import { Prisma, WalletOwnerType, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";
import { env } from "../../config/env.config";
import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

type DeliveryDeliveredEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  CustomerId?: string;
  ShipperId?: string;
  MerchantId?: string;
  Data?: Record<string, unknown>;
  data?: Record<string, unknown>;
  DeliveryFee?: number | string;
  DistanceKm?: number | string;
  DeliveryAt?: string;
  Status?: string;
  ProofFileKey?: string | null;
  Note?: string | null;
  orderId?: string;
  orderNumber?: string;
  customerId?: string;
  shipperId?: string;
  merchantId?: string;
  deliveryFee?: number | string;
  distanceKm?: number | string;
  deliveryAt?: string;
  status?: string;
  proofFileKey?: string | null;
  note?: string | null;
};

export class DeliveryDeliveredConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "wallet-delivery-delivered-queue";
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    const channel = await this.rabbitMqClient.createConsumerQueue(this.queueName, ["delivery.delivered"]);

    await channel.consume(
      this.queueName,
      (message: RabbitConsumerMessage | null) => {
        void this.handleMessage(message, channel);
      },
      {
        noAck: false,
      },
    );

    this.running = true;
    logger.info("Wallet delivery.delivered consumer started");
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
      const rawPayload = JSON.parse(message.content.toString("utf8")) as DeliveryDeliveredEventPayload;
      const payload = this.unwrapPayload(rawPayload);
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const shipperId = payload.ShipperId ?? payload.shipperId;
      const merchantId = payload.MerchantId ?? payload.merchantId;
      const deliveryFee = Number(payload.DeliveryFee ?? payload.deliveryFee ?? 0);
      const status = String(payload.Status ?? payload.status ?? "").toLowerCase();
      const messageId = message.properties.messageId ?? orderId ?? `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !shipperId || !merchantId) {
        throw new Error("Invalid delivery.delivered payload");
      }

      if (status && status !== "delivered") {
        logger.warn(`Skipping delivery.delivered event with status "${status}" for order ${orderNumber}`);
        channel.ack(message);
        return;
      }

      if (Number.isNaN(deliveryFee) || deliveryFee <= 0) {
        throw new Error("Invalid delivery fee in delivery.delivered payload");
      }

      const exists = await prisma.walletEventInbox.findUnique({
        where: { messageId },
      });

      if (exists) {
        channel.ack(message);
        return;
      }

      const commissionAmount = this.roundMoney(
        (deliveryFee * env.WALLET_SHIPPER_COMMISSION_RATE) / 100,
      );

      await prisma.$transaction(async (tx) => {
        await tx.walletEventInbox.create({
          data: {
            messageId,
            eventType: "delivery.delivered",
            aggregateType: "Delivery",
            aggregateId: orderId,
            payload: {
              ...payload,
              commissionRate: env.WALLET_SHIPPER_COMMISSION_RATE,
              commissionAmount,
            } as Prisma.InputJsonValue,
            processedAt: new Date(),
          },
        });

        const shipperWallet = await tx.wallet.upsert({
          where: {
            ownerType_ownerId: {
              ownerType: WalletOwnerType.SHIPPER,
              ownerId: shipperId,
            },
          },
          create: {
            ownerType: WalletOwnerType.SHIPPER,
            ownerId: shipperId,
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

        const shipperBalanceBefore = Number(shipperWallet.balance);
        const shipperBalanceAfter = shipperBalanceBefore - commissionAmount;
        const adminBalanceBefore = Number(adminWallet.balance);
        const adminBalanceAfter = adminBalanceBefore + commissionAmount;
        const shipperNegativeSince =
          shipperBalanceAfter < 0
            ? shipperWallet.negativeSince ?? new Date()
            : null;
        const adminNegativeSince =
          adminBalanceAfter < 0
            ? adminWallet.negativeSince ?? new Date()
            : null;

        await tx.walletTransaction.create({
          data: {
            walletId: shipperWallet.id,
            type: WalletTransactionType.COMMISSION,
            amount: new Prisma.Decimal(commissionAmount),
            balanceBefore: new Prisma.Decimal(shipperBalanceBefore),
            balanceAfter: new Prisma.Decimal(shipperBalanceAfter),
            referenceId: orderId,
            referenceType: "delivery",
            referenceCode: orderNumber,
            description: `Delivery commission for order ${orderNumber}`,
            status: WalletTransactionStatus.COMPLETED,
            idempotencyKey: messageId,
            metadata: {
              ownerType: "SHIPPER",
              shipperId,
              merchantId,
              orderNumber,
              deliveryFee,
              commissionRate: env.WALLET_SHIPPER_COMMISSION_RATE,
              commissionAmount,
            } as Prisma.InputJsonValue,
          },
        });

        await tx.wallet.update({
          where: { id: shipperWallet.id },
          data: {
            balance: new Prisma.Decimal(shipperBalanceAfter),
            negativeSince: shipperNegativeSince,
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
            referenceType: "delivery",
            referenceCode: orderNumber,
            description: `Platform commission from delivery ${orderNumber}`,
            status: WalletTransactionStatus.COMPLETED,
            idempotencyKey: `${messageId}:admin`,
            metadata: {
              ownerType: "ADMIN",
              shipperId,
              merchantId,
              orderNumber,
              deliveryFee,
              commissionRate: env.WALLET_SHIPPER_COMMISSION_RATE,
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
      logger.error("Failed to process delivery.delivered event", error);
      channel.nack(message, false, false);
    }
  }

  private unwrapPayload(payload: DeliveryDeliveredEventPayload) {
    const data = payload.Data ?? payload.data;

    if (data && typeof data === "object" && !Array.isArray(data)) {
      return {
        ...payload,
        ...data,
        Data: data,
      } as DeliveryDeliveredEventPayload & Record<string, unknown>;
    }

    return payload;
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
