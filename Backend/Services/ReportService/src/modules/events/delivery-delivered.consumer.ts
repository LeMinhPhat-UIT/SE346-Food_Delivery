import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

const DELIVERY_STATUS_DELIVERED = "DELIVERED";
const ORDER_STATUS_DELIVERED = "DELIVERED";
const ORDER_PAYMENT_STATUS_PAID = "PAID";

type DeliveryDeliveredEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  CustomerId?: string;
  ShipperId?: string;
  MerchantId?: string;
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
  private readonly queueName = "report-delivery-delivered-queue";
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
      { noAck: false },
    );

    this.running = true;
    logger.info("Report delivery.delivered consumer started");
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
      const payload = JSON.parse(message.content.toString("utf8")) as DeliveryDeliveredEventPayload;
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const shipperId = payload.ShipperId ?? payload.shipperId;
      const merchantId = payload.MerchantId ?? payload.merchantId;
      const deliveryFee = this.toDecimal(payload.DeliveryFee ?? payload.deliveryFee);
      const distanceKm = this.toDecimal(payload.DistanceKm ?? payload.distanceKm);
      const deliveryAt = payload.DeliveryAt ?? payload.deliveryAt;
      const messageId = message.properties.messageId ?? orderId ?? `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !merchantId) {
        throw new Error("Invalid delivery.delivered payload");
      }

      const inboxExists = await prisma.reportEventInbox.findUnique({
        where: { messageId },
      });

      if (inboxExists) {
        channel.ack(message);
        return;
      }

      const eventDate = deliveryAt ? new Date(deliveryAt) : new Date();

      await prisma.$transaction(async (tx) => {
        await tx.reportEventInbox.create({
          data: {
            messageId,
            eventType: "delivery.delivered",
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

        if (!existingDelivery) {
          await tx.reportDeliveryFact.create({
            data: {
              deliveryId: orderId,
              orderId,
              shipperId: shipperId ?? null,
              merchantId: merchantId ?? orderFact?.merchantId ?? "00000000-0000-0000-0000-000000000000",
              status: DELIVERY_STATUS_DELIVERED,
              assignedAt: eventDate,
              deliveredAt: eventDate,
              actualDistanceKm: distanceKm,
              deliveryFee,
              createdAt: eventDate,
            },
          });
        } else {
          await tx.reportDeliveryFact.update({
            where: { deliveryId: orderId },
            data: {
              shipperId: shipperId ?? existingDelivery.shipperId,
              merchantId: merchantId ?? existingDelivery.merchantId,
              status: DELIVERY_STATUS_DELIVERED,
              assignedAt: existingDelivery.assignedAt ?? eventDate,
              pickedUpAt: existingDelivery.pickedUpAt ?? eventDate,
              deliveredAt: existingDelivery.deliveredAt ?? eventDate,
              actualDistanceKm: distanceKm || existingDelivery.actualDistanceKm,
              deliveryFee: deliveryFee || existingDelivery.deliveryFee,
            },
          });
        }

        await tx.reportOrderFact.update({
          where: { orderId },
          data: {
            shipperId: shipperId ?? existingDelivery?.shipperId ?? null,
            orderStatus: ORDER_STATUS_DELIVERED,
            paymentStatus: ORDER_PAYMENT_STATUS_PAID,
            deliveredAt: eventDate,
            deliveryFee,
          },
        });

        await this.rebuildAdminDailyMetric(tx, eventDate);
        await this.rebuildMerchantDailyMetric(tx, eventDate, merchantId ?? orderFact?.merchantId ?? "00000000-0000-0000-0000-000000000000");
        if (shipperId) {
          await this.rebuildShipperDailyMetric(tx, eventDate, shipperId);
        }
      });

      channel.ack(message);
    } catch (error) {
      logger.error("Failed to process delivery.delivered event", error);
      channel.nack(message, false, false);
    }
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
      },
    });

    const summary = orders.reduce(
      (acc: any, row: any) => ({
        grossRevenue: acc.grossRevenue + this.toDecimal(row.subtotal) - this.toDecimal(row.discountAmount) - this.toDecimal(row.deliveryFee),
        netRevenue: acc.netRevenue + this.toDecimal(row.totalAmount),
        orderCount: acc.orderCount + 1,
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === ORDER_PAYMENT_STATUS_PAID ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.orderStatus === "CANCELLED" ? 1 : 0),
        deliveryFeeTotal: acc.deliveryFeeTotal + this.toDecimal(row.deliveryFee),
        discountTotal: acc.discountTotal + this.toDecimal(row.discountAmount),
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

    const summary = orders.reduce(
      (acc: any, row: any) => ({
        grossRevenue: acc.grossRevenue + this.toDecimal(row.subtotal) - this.toDecimal(row.discountAmount) - this.toDecimal(row.deliveryFee),
        netRevenue: acc.netRevenue + this.toDecimal(row.totalAmount),
        orderCount: acc.orderCount + 1,
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === ORDER_PAYMENT_STATUS_PAID ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.orderStatus === "CANCELLED" ? 1 : 0),
        subtotalRevenue: acc.subtotalRevenue + this.toDecimal(row.subtotal),
        deliveryFeeRevenue: acc.deliveryFeeRevenue + this.toDecimal(row.deliveryFee),
        discountTotal: acc.discountTotal + this.toDecimal(row.discountAmount),
        voucherUsageCount: acc.voucherUsageCount + (this.toDecimal(row.discountAmount) > 0 ? 1 : 0),
      }),
      {
        grossRevenue: 0,
        netRevenue: 0,
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

    const summary = deliveries.reduce(
      (acc: any, row: any) => ({
        assignedOrderCount: acc.assignedOrderCount + (row.assignedAt ? 1 : 0),
        pickedUpOrderCount: acc.pickedUpOrderCount + (row.pickedUpAt ? 1 : 0),
        deliveredOrderCount: acc.deliveredOrderCount + (row.deliveredAt ? 1 : 0),
        cancelledOrderCount: acc.cancelledOrderCount + (row.status === "CANCELLED" ? 1 : 0),
        completionRate: acc.completionRate,
        avgDeliveryTimeMinutes: acc.avgDeliveryTimeMinutes,
        totalDistanceKm: acc.totalDistanceKm + this.toDecimal(row.actualDistanceKm),
        deliveryFeeHandled: acc.deliveryFeeHandled + this.toDecimal(row.deliveryFee),
      }),
      {
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
