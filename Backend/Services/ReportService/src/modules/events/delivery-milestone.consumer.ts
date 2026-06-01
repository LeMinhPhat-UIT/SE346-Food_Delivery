import { prisma } from "../../prisma/prisma.client";
import { env } from "../../config/env.config";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

const DELIVERY_STATUS_DELIVERING = "DELIVERING";
const DELIVERY_STATUS_DELIVERED = "DELIVERED";
const DELIVERY_STATUS_CANCELLED = "CANCELLED";
const ORDER_STATUS_PICKED_UP = "PICKED_UP";
const ORDER_STATUS_DELIVERED = "DELIVERED";
const ORDER_STATUS_CANCELLED = "CANCELLED";
const ORDER_PAYMENT_STATUS_PAID = "PAID";

type DeliveryMilestoneEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  CustomerId?: string;
  ShipperId?: string;
  Data?: Record<string, unknown>;
  data?: Record<string, unknown>;
  Milestone?: "PickedUp" | "Delivered" | string;
  ProofFileKey?: string | null;
  Note?: string | null;
  orderId?: string;
  orderNumber?: string;
  customerId?: string;
  shipperId?: string;
  milestone?: "PickedUp" | "Delivered" | string;
  proofFileKey?: string | null;
  note?: string | null;
};

export class DeliveryMilestoneConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "report-delivery-milestone-queue";
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    const channel = await this.rabbitMqClient.createConsumerQueue(this.queueName, ["delivery.milestone"]);

    await channel.consume(
      this.queueName,
      (message: RabbitConsumerMessage | null) => {
        void this.handleMessage(message, channel);
      },
      { noAck: false },
    );

    this.running = true;
    logger.info("Report delivery.milestone consumer started");
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
      const rawPayload = JSON.parse(message.content.toString("utf8")) as DeliveryMilestoneEventPayload;
      const payload = this.unwrapPayload(rawPayload);
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const shipperId = payload.ShipperId ?? payload.shipperId;
      const milestone = String(payload.Milestone ?? payload.milestone ?? "").toLowerCase();
      const messageId = message.properties.messageId ?? orderId ?? `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !shipperId) {
        throw new Error("Invalid delivery.milestone payload");
      }

      const inboxExists = await prisma.reportEventInbox.findUnique({
        where: { messageId },
      });

      if (inboxExists) {
        channel.ack(message);
        return;
      }

      const eventDate = new Date(message.properties.timestamp ? new Date(message.properties.timestamp) : new Date());

      await prisma.$transaction(async (tx) => {
        await tx.reportEventInbox.create({
          data: {
            messageId,
            eventType: "delivery.milestone",
            aggregateType: "Delivery",
            aggregateId: orderId,
            payload: {
              ...payload,
              ingestedAt: new Date().toISOString(),
            } as any,
            processedAt: new Date(),
          },
        });

        const orderFact = await tx.reportOrderFact.findUnique({
          where: { orderId },
          select: { merchantId: true },
        });

        const existingDelivery = await tx.reportDeliveryFact.findUnique({
          where: { deliveryId: orderId },
        });

        const baseData = {
          deliveryId: orderId,
          orderId,
          shipperId,
          merchantId: orderFact?.merchantId ?? "00000000-0000-0000-0000-000000000000",
        };

        if (!existingDelivery) {
          await tx.reportDeliveryFact.create({
            data: {
              ...baseData,
              status: milestone === "delivered" ? DELIVERY_STATUS_DELIVERED : DELIVERY_STATUS_DELIVERING,
              assignedAt: eventDate,
              pickedUpAt: milestone === "pickedup" ? eventDate : null,
              deliveredAt: milestone === "delivered" ? eventDate : null,
              createdAt: eventDate,
            },
          });
        } else {
          await tx.reportDeliveryFact.update({
            where: { deliveryId: orderId },
            data: {
              shipperId,
              merchantId: orderFact?.merchantId ?? existingDelivery.merchantId,
              status: milestone === "delivered" ? DELIVERY_STATUS_DELIVERED : DELIVERY_STATUS_DELIVERING,
              assignedAt: existingDelivery.assignedAt ?? eventDate,
              pickedUpAt: milestone === "pickedup" ? (existingDelivery.pickedUpAt ?? eventDate) : existingDelivery.pickedUpAt,
              deliveredAt: milestone === "delivered" ? (existingDelivery.deliveredAt ?? eventDate) : existingDelivery.deliveredAt,
            },
          });
        }

        if (milestone === "delivered") {
          await tx.reportOrderFact.update({
            where: { orderId },
            data: {
              shipperId,
              orderStatus: ORDER_STATUS_DELIVERED,
              paymentStatus: ORDER_PAYMENT_STATUS_PAID,
              deliveredAt: eventDate,
            },
          });
        } else if (milestone === "pickedup") {
          await tx.reportOrderFact.updateMany({
            where: { orderId },
            data: {
              shipperId,
              orderStatus: ORDER_STATUS_PICKED_UP,
              pickedUpAt: eventDate,
            },
          });
        }

        await this.rebuildAdminDailyMetric(tx, eventDate);
        if (orderFact?.merchantId) {
          await this.rebuildMerchantDailyMetric(tx, eventDate, orderFact.merchantId);
        }
        await this.rebuildShipperDailyMetric(tx, eventDate, shipperId);
      });

      channel.ack(message);
    } catch (error) {
      logger.error("Failed to process delivery.milestone event", error);
      channel.nack(message, false, false);
    }
  }

  private unwrapPayload(payload: DeliveryMilestoneEventPayload) {
    const data = payload.Data ?? payload.data;

    if (data && typeof data === "object" && !Array.isArray(data)) {
      return {
        ...payload,
        ...data,
        Data: data,
      } as DeliveryMilestoneEventPayload & Record<string, unknown>;
    }

    return payload;
  }

  private async rebuildAdminDailyMetric(tx: any, metricDate: Date) {
    const range = this.buildDateRange(metricDate);
        const orders = await tx.reportOrderFact.findMany({
          where: {
            createdAt: {
              gte: range.from,
              lt: range.to,
            },
          },
          select: {
            customerId: true,
            merchantId: true,
            shipperId: true,
            paymentMethod: true,
            paymentStatus: true,
            orderStatus: true,
            subtotal: true,
            deliveryFee: true,
            discountAmount: true,
            totalAmount: true,
          },
        });

        const deliveries = await tx.reportDeliveryFact.findMany({
          where: {
            createdAt: {
              gte: range.from,
              lt: range.to,
            },
          },
          select: {
            shipperId: true,
            status: true,
            deliveryFee: true,
            deliveredAt: true,
          },
        });

        const merchantCommissionTotal = orders.reduce(
          (acc: number, row: any) =>
            acc +
            this.roundMoney(
              this.toDecimal(row.totalAmount) * (env.WALLET_MERCHANT_COMMISSION_RATE / 100),
            ),
          0,
        );

        const shipperCommissionTotal = deliveries.reduce(
          (acc: number, row: any) =>
            acc +
            (row.deliveredAt || row.status === DELIVERY_STATUS_DELIVERED
              ? this.roundMoney(
                  this.toDecimal(row.deliveryFee) * (env.WALLET_SHIPPER_COMMISSION_RATE / 100),
                )
              : 0),
          0,
        );

        const summary = orders.reduce(
          (acc: any, row: any) => ({
        grossRevenue: this.roundMoney(acc.grossRevenue + this.toDecimal(row.totalAmount)),
        netRevenue: acc.netRevenue,
        platformRevenue: acc.platformRevenue,
        merchantCommissionTotal: acc.merchantCommissionTotal,
        shipperCommissionTotal: acc.shipperCommissionTotal,
        orderCount: acc.orderCount + 1,
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === ORDER_PAYMENT_STATUS_PAID ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.orderStatus === ORDER_STATUS_CANCELLED ? 1 : 0),
        deliveryFeeTotal: this.roundMoney(acc.deliveryFeeTotal + this.toDecimal(row.deliveryFee)),
        discountTotal: this.roundMoney(acc.discountTotal + this.toDecimal(row.discountAmount)),
        voucherUsageCount: acc.voucherUsageCount + (this.toDecimal(row.discountAmount) > 0 ? 1 : 0),
        codOrderCount: acc.codOrderCount + (row.paymentMethod === "COD" ? 1 : 0),
        vnpayOrderCount: acc.vnpayOrderCount + (row.paymentMethod === "VNPAY" ? 1 : 0),
        uniqueCustomers: acc.uniqueCustomers,
        uniqueMerchants: acc.uniqueMerchants,
        uniqueShippers: acc.uniqueShippers,
      }),
      {
        grossRevenue: 0,
        netRevenue: 0,
        platformRevenue: 0,
        merchantCommissionTotal: 0,
        shipperCommissionTotal: 0,
        orderCount: 0,
        paidOrderCount: 0,
        cancelledOrderCount: 0,
        deliveryFeeTotal: 0,
        discountTotal: 0,
        voucherUsageCount: 0,
        codOrderCount: 0,
        vnpayOrderCount: 0,
        uniqueCustomers: 0,
        uniqueMerchants: 0,
        uniqueShippers: 0,
      },
    );

    summary.uniqueCustomers = new Set(orders.map((row: any) => row.customerId)).size;
    summary.uniqueMerchants = new Set(orders.map((row: any) => row.merchantId)).size;
    summary.uniqueShippers = new Set(deliveries.map((row: any) => row.shipperId).filter(Boolean)).size;
    summary.merchantCommissionTotal = this.roundMoney(merchantCommissionTotal);
    summary.shipperCommissionTotal = this.roundMoney(shipperCommissionTotal);
    summary.platformRevenue = this.roundMoney(merchantCommissionTotal + shipperCommissionTotal);
    summary.netRevenue = summary.platformRevenue;

    await tx.reportAdminDailyMetric.upsert({
      where: { metricDate },
      create: {
        metricDate,
        ...summary,
      },
      update: summary,
    });
  }

  private async rebuildMerchantDailyMetric(tx: any, metricDate: Date, merchantId: string) {
    const range = this.buildDateRange(metricDate);
    const orders = await tx.reportOrderFact.findMany({
      where: {
        merchantId,
        createdAt: {
          gte: range.from,
          lt: range.to,
        },
      },
      select: {
        subtotal: true,
        deliveryFee: true,
        discountAmount: true,
        totalAmount: true,
        paymentStatus: true,
        orderStatus: true,
      },
    });

    const merchantCommissionTotal = orders.reduce(
      (acc: number, row: any) =>
        acc +
        this.roundMoney(
          this.toDecimal(row.totalAmount) * (env.WALLET_MERCHANT_COMMISSION_RATE / 100),
        ),
      0,
    );

    const summary = orders.reduce(
      (acc: any, row: any) => ({
        grossRevenue: this.roundMoney(
          acc.grossRevenue + this.toDecimal(row.subtotal) - this.toDecimal(row.discountAmount),
        ),
        netRevenue: acc.netRevenue,
        merchantCommissionTotal: acc.merchantCommissionTotal,
        orderCount: acc.orderCount + 1,
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === ORDER_PAYMENT_STATUS_PAID ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.orderStatus === ORDER_STATUS_CANCELLED ? 1 : 0),
        subtotalRevenue: this.roundMoney(acc.subtotalRevenue + this.toDecimal(row.subtotal)),
        deliveryFeeRevenue: this.roundMoney(acc.deliveryFeeRevenue + this.toDecimal(row.deliveryFee)),
        discountTotal: this.roundMoney(acc.discountTotal + this.toDecimal(row.discountAmount)),
        voucherUsageCount: acc.voucherUsageCount + (this.toDecimal(row.discountAmount) > 0 ? 1 : 0),
      }),
      {
        grossRevenue: 0,
        netRevenue: 0,
        merchantCommissionTotal: 0,
        orderCount: 0,
        paidOrderCount: 0,
        cancelledOrderCount: 0,
        subtotalRevenue: 0,
        deliveryFeeRevenue: 0,
        discountTotal: 0,
        voucherUsageCount: 0,
      },
    );

    const avgOrderValue = summary.orderCount ? this.roundMoney(summary.netRevenue / summary.orderCount) : 0;
    summary.merchantCommissionTotal = this.roundMoney(merchantCommissionTotal);
    summary.netRevenue = this.roundMoney(summary.grossRevenue - merchantCommissionTotal);

    await tx.reportMerchantDailyMetric.upsert({
      where: {
        metricDate_merchantId: {
          metricDate,
          merchantId,
        },
      },
      create: {
        metricDate,
        merchantId,
        ...summary,
        avgOrderValue,
      },
      update: {
        ...summary,
        avgOrderValue,
      },
    });
  }

  private async rebuildShipperDailyMetric(tx: any, metricDate: Date, shipperId: string) {
    const range = this.buildDateRange(metricDate);
    const deliveries = await tx.reportDeliveryFact.findMany({
      where: {
        shipperId,
        createdAt: {
          gte: range.from,
          lt: range.to,
        },
      },
      select: {
        assignedAt: true,
        pickedUpAt: true,
        deliveredAt: true,
        status: true,
        deliveryFee: true,
        actualDistanceKm: true,
      },
    });

    const shipperCommissionTotal = deliveries.reduce(
      (acc: number, row: any) =>
        acc +
        (row.deliveredAt || row.status === DELIVERY_STATUS_DELIVERED
          ? this.roundMoney(
              this.toDecimal(row.deliveryFee) * (env.WALLET_SHIPPER_COMMISSION_RATE / 100),
            )
          : 0),
      0,
    );

    const summary = deliveries.reduce(
      (acc: any, row: any) => ({
        grossRevenue: this.roundMoney(acc.grossRevenue + this.toDecimal(row.deliveryFee)),
        netEarnings: acc.netEarnings,
        shipperCommissionTotal: acc.shipperCommissionTotal,
        assignedOrderCount: acc.assignedOrderCount + (row.assignedAt ? 1 : 0),
        pickedUpOrderCount: acc.pickedUpOrderCount + (row.pickedUpAt ? 1 : 0),
        deliveredOrderCount: acc.deliveredOrderCount + (row.deliveredAt ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.status === DELIVERY_STATUS_CANCELLED ? 1 : 0),
        completionRate: acc.completionRate,
        avgDeliveryTimeMinutes: acc.avgDeliveryTimeMinutes,
        totalDistanceKm: this.roundMoney(acc.totalDistanceKm + this.toDecimal(row.actualDistanceKm)),
        deliveryFeeHandled: this.roundMoney(acc.deliveryFeeHandled + this.toDecimal(row.deliveryFee)),
      }),
      {
        grossRevenue: 0,
        netEarnings: 0,
        shipperCommissionTotal: 0,
        assignedOrderCount: 0,
        pickedUpOrderCount: 0,
        deliveredOrderCount: 0,
        cancelledOrderCount: 0,
        completionRate: 0,
        avgDeliveryTimeMinutes: 0,
        totalDistanceKm: 0,
        deliveryFeeHandled: 0,
      },
    );

    const completionRate = summary.assignedOrderCount
      ? this.roundMoney((summary.deliveredOrderCount / summary.assignedOrderCount) * 100)
      : 0;
    summary.shipperCommissionTotal = this.roundMoney(shipperCommissionTotal);
    summary.netEarnings = this.roundMoney(summary.grossRevenue - shipperCommissionTotal);

    await tx.reportShipperDailyMetric.upsert({
      where: {
        metricDate_shipperId: {
          metricDate,
          shipperId,
        },
      },
      create: {
        metricDate,
        shipperId,
        ...summary,
        completionRate,
      },
      update: {
        ...summary,
        completionRate,
      },
    });
  }

  private buildDateRange(date: Date) {
    const from = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
    const to = new Date(from.getTime() + 24 * 60 * 60 * 1000);
    return { from, to };
  }

  private toDecimal(value: any) {
    return Number(value ?? 0);
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
