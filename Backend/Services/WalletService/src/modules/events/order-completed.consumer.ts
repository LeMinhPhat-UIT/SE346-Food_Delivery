import { Prisma, WalletOwnerType, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";
import { env } from "../../config/env.config";
import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

type OrderCompletedEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  MerchantId?: string;
  MerchantStoreName?: string;
  UserId?: string;
  Data?: Record<string, unknown>;
  data?: Record<string, unknown>;
  TotalAmount?: number;
  PaymentMethod?: string;
  Note?: string | null;
  orderId?: string;
  orderNumber?: string;
  merchantId?: string;
  merchantStoreName?: string;
  userId?: string;
  totalAmount?: number;
  paymentMethod?: string;
  note?: string | null;
};

export class OrderCompletedConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "wallet-order-completed-queue";
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    const channel = await this.rabbitMqClient.createConsumerQueue(this.queueName, ["order.completed"]);

    await channel.consume(this.queueName, (message: RabbitConsumerMessage | null) => {
      void this.handleMessage(message, channel);
    }, {
      noAck: false,
    });

    this.running = true;
    logger.info("Wallet order.completed consumer started");
  }

  async stop() {
    this.running = false;
    await this.rabbitMqClient.close();
  }

  private async handleMessage(message: RabbitConsumerMessage | null, channel: any) {
    if (!message) {
      return;
    }

    try {
      const rawPayload = JSON.parse(message.content.toString("utf8")) as OrderCompletedEventPayload;
      const payload = this.unwrapPayload(rawPayload);
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const merchantId = payload.MerchantId ?? payload.merchantId;
      const userId = payload.UserId ?? payload.userId;
      const totalAmount = Number(payload.TotalAmount ?? payload.totalAmount ?? 0);
      const paymentMethod = String(payload.PaymentMethod ?? payload.paymentMethod ?? "");
      const messageId = message.properties.messageId ?? orderId ?? `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !merchantId || !userId) {
        throw new Error("Invalid order.completed payload");
      }

      const exists = await prisma.walletEventInbox.findUnique({
        where: { messageId },
      });

      if (exists) {
        channel.ack(message);
        return;
      }

      const commissionAmount = this.roundMoney(
        (totalAmount * env.WALLET_MERCHANT_COMMISSION_RATE) / 100,
      );

      await prisma.$transaction(async (tx) => {
        await tx.walletEventInbox.create({
          data: {
            messageId,
            eventType: "order.completed",
            aggregateType: "Order",
            aggregateId: orderId,
            payload: {
              ...payload,
              commissionRate: env.WALLET_MERCHANT_COMMISSION_RATE,
              commissionAmount,
            } as Prisma.InputJsonValue,
            processedAt: new Date(),
          },
        });

        const merchantWallet = await tx.wallet.upsert({
          where: {
            ownerType_ownerId: {
              ownerType: WalletOwnerType.MERCHANT,
              ownerId: merchantId,
            },
          },
          create: {
            ownerType: WalletOwnerType.MERCHANT,
            ownerId: merchantId,
          },
          update: {},
        });

        const adminWallet = await tx.wallet.upsert({
          where: {
            ownerType_ownerId: {
              ownerType: WalletOwnerType.ADMIN,
              ownerId: env.WALLET_PLATFORM_OWNER_ID,
            },
          },
          create: {
            ownerType: WalletOwnerType.ADMIN,
            ownerId: env.WALLET_PLATFORM_OWNER_ID,
          },
          update: {},
        });

        const merchantBalanceBefore = Number(merchantWallet.balance);
        const merchantBalanceAfter = merchantBalanceBefore - commissionAmount;
        const adminBalanceBefore = Number(adminWallet.balance);
        const adminBalanceAfter = adminBalanceBefore + commissionAmount;
        const merchantNegativeSince =
          merchantBalanceAfter < 0
            ? merchantWallet.negativeSince ?? new Date()
            : null;
        const adminNegativeSince =
          adminBalanceAfter < 0
            ? adminWallet.negativeSince ?? new Date()
            : null;

        await tx.walletTransaction.create({
          data: {
            walletId: merchantWallet.id,
            type: WalletTransactionType.COMMISSION,
            amount: new Prisma.Decimal(commissionAmount),
            balanceBefore: new Prisma.Decimal(merchantBalanceBefore),
            balanceAfter: new Prisma.Decimal(merchantBalanceAfter),
            referenceId: orderId,
            referenceType: "order",
            referenceCode: orderNumber,
            description: `Commission for order ${orderNumber}`,
            status: WalletTransactionStatus.COMPLETED,
            idempotencyKey: messageId,
            metadata: {
              ownerType: "MERCHANT",
              paymentMethod,
              merchantId,
              userId,
              orderNumber,
              commissionRate: env.WALLET_MERCHANT_COMMISSION_RATE,
              commissionAmount,
            } as Prisma.InputJsonValue,
          },
        });

        await tx.wallet.update({
          where: { id: merchantWallet.id },
          data: {
            balance: new Prisma.Decimal(merchantBalanceAfter),
            negativeSince: merchantNegativeSince,
          },
        });

        await tx.walletTransaction.create({
          data: {
            walletId: adminWallet.id,
            type: WalletTransactionType.COMMISSION,
            amount: new Prisma.Decimal(commissionAmount),
            balanceBefore: new Prisma.Decimal(adminBalanceBefore),
            balanceAfter: new Prisma.Decimal(adminBalanceAfter),
            referenceId: orderId,
            referenceType: "order",
            referenceCode: orderNumber,
            description: `Platform commission from order ${orderNumber}`,
            status: WalletTransactionStatus.COMPLETED,
            idempotencyKey: `${messageId}:admin`,
            metadata: {
              ownerType: "ADMIN",
              paymentMethod,
              merchantId,
              userId,
              orderNumber,
              commissionRate: env.WALLET_MERCHANT_COMMISSION_RATE,
              commissionAmount,
            } as Prisma.InputJsonValue,
          },
        });

        await tx.wallet.update({
          where: { id: adminWallet.id },
          data: {
            balance: new Prisma.Decimal(adminBalanceAfter),
            negativeSince: adminNegativeSince,
          },
        });
      });

      channel.ack(message);
    } catch (error) {
      logger.error("Failed to process order.completed event", error);
      channel.nack(message, false, false);
    }
  }

  private unwrapPayload(payload: OrderCompletedEventPayload) {
    const data = payload.Data ?? payload.data;

    if (data && typeof data === "object" && !Array.isArray(data)) {
      return {
        ...payload,
        ...data,
        Data: data,
      } as OrderCompletedEventPayload & Record<string, unknown>;
    }

    return payload;
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
