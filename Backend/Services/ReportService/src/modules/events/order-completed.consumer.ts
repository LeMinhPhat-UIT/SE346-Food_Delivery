import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

type OrderCompletedEventPayload = {
  OrderId?: string;
  OrderNumber?: string;
  MerchantId?: string;
  MerchantStoreName?: string;
  UserId?: string;
  TotalAmount?: number | string;
  PaymentMethod?: string;
  Note?: string | null;
  Subtotal?: number | string;
  DeliveryFee?: number | string;
  DiscountAmount?: number | string;
  Items?: Array<Record<string, unknown>> | null;
  orderId?: string;
  orderNumber?: string;
  merchantId?: string;
  merchantStoreName?: string;
  userId?: string;
  totalAmount?: number | string;
  paymentMethod?: string;
  note?: string | null;
  subtotal?: number | string;
  deliveryFee?: number | string;
  discountAmount?: number | string;
  items?: Array<Record<string, unknown>> | null;
};

export class OrderCompletedConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "report-order-completed-queue";
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    const channel = await this.rabbitMqClient.createConsumerQueue(this.queueName, ["order.completed"]);

    await channel.consume(
      this.queueName,
      (message: RabbitConsumerMessage | null) => {
        void this.handleMessage(message, channel);
      },
      { noAck: false },
    );

    this.running = true;
    logger.info("Report order.completed consumer started");
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
      const payload = JSON.parse(message.content.toString("utf8")) as OrderCompletedEventPayload;
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const merchantId = payload.MerchantId ?? payload.merchantId;
      const merchantName = payload.MerchantStoreName ?? payload.merchantStoreName ?? "Unknown merchant";
      const customerId = payload.UserId ?? payload.userId;
      const paymentMethod = String(payload.PaymentMethod ?? payload.paymentMethod ?? "COD").toUpperCase();
      const totalAmount = this.toDecimal(payload.TotalAmount ?? payload.totalAmount);
      const subtotal = this.toDecimal(payload.Subtotal ?? payload.subtotal ?? totalAmount);
      const deliveryFee = this.toDecimal(payload.DeliveryFee ?? payload.deliveryFee ?? 0);
      const discountAmount = this.toDecimal(payload.DiscountAmount ?? payload.discountAmount ?? 0);
      const messageId = message.properties.messageId ?? orderId ?? `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !merchantId || !customerId) {
        throw new Error("Invalid order.completed payload");
      }

      const inboxExists = await prisma.reportEventInbox.findUnique({
        where: { messageId },
      });

      if (inboxExists) {
        channel.ack(message);
        return;
      }

      const createdAt = this.toReportDate(message.properties.timestamp ? new Date(message.properties.timestamp) : new Date());

      await prisma.$transaction(async (tx: any) => {
        await tx.reportEventInbox.create({
          data: {
            messageId,
            eventType: "order.completed",
            aggregateType: "Order",
            aggregateId: orderId,
            payload: {
              ...payload,
              ingestedAt: new Date().toISOString(),
            },
            processedAt: new Date(),
          },
        });

        const existingOrder = await tx.reportOrderFact.findUnique({
          where: { orderId },
        });

        if (!existingOrder) {
          await tx.reportOrderFact.create({
            data: {
              orderId,
              orderNumber,
              customerId,
              merchantId,
              merchantName,
              shipperId: null,
              paymentMethod: paymentMethod === "VNPAY" ? "VNPAY" : "COD",
              paymentStatus: "PAID",
              orderStatus: "CONFIRMED",
              subtotal,
              deliveryFee,
              discountAmount,
              totalAmount,
              voucherId: null,
              voucherCode: null,
              createdAt,
            },
          });
        }

        const existingPayment = await tx.reportPaymentFact.findUnique({
          where: { paymentId: orderId },
        });

        if (!existingPayment) {
          await tx.reportPaymentFact.create({
            data: {
              paymentId: orderId,
              orderId,
              method: paymentMethod === "VNPAY" ? "VNPAY" : "COD",
              status: "COMPLETED",
              amount: totalAmount,
              provider: paymentMethod,
              paidAt: createdAt,
            },
          });
        }

        const items = payload.Items ?? payload.items ?? [];
        for (const item of items) {
          const productId = this.getString(item, "ProductId", "productId");
          if (!productId) {
            continue;
          }

          await tx.reportOrderItemFact.create({
            data: {
              orderId,
              productId,
              productName: this.getString(item, "ProductName", "productName") ?? "Unknown product",
              productImage: this.getString(item, "ProductImage", "productImage"),
              unitPrice: this.toDecimal(this.getValue(item, "UnitPrice", "unitPrice")),
              quantity: Number(this.getValue(item, "Quantity", "quantity") ?? 1),
              selectedOptions: this.getValue(item, "SelectedOptions", "selectedOptions") ?? null,
              note: this.getString(item, "Note", "note"),
              createdAt,
            },
          });
        }

        await this.rebuildAdminDailyMetric(tx, createdAt);
        await this.rebuildMerchantDailyMetric(tx, createdAt, merchantId);
      });

      channel.ack(message);
    } catch (error) {
      logger.error("Failed to process order.completed event", error);
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
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === "PAID" ? 1 : 0),
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
        paidOrderCount: acc.paidOrderCount + (row.paymentStatus === "PAID" ? 1 : 0),
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

  private getValue(obj: Record<string, any>, ...keys: string[]) {
    for (const key of keys) {
      if (typeof obj[key] !== "undefined") {
        return obj[key];
      }
    }

    return undefined;
  }

  private getString(obj: Record<string, any>, ...keys: string[]) {
    const value = this.getValue(obj, ...keys);
    return typeof value === "string" ? value : null;
  }

  private buildDateRange(date: Date) {
    const from = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
    const to = new Date(from.getTime() + 24 * 60 * 60 * 1000);
    return { from, to };
  }

  private toReportDate(date: Date) {
    return new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
  }

  private toDecimal(value: any) {
    return Number(value ?? 0);
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
