import { Prisma } from "@prisma/client";
import { env } from "../../config/env.config";
import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

type OrderCompletedEventPayload = {
  EventId?: string;
  eventId?: string;
  RoutingKey?: string;
  routingKey?: string;
  Data?: Record<string, unknown>;
  data?: Record<string, unknown>;
  OrderId?: string;
  orderId?: string;
  OrderNumber?: string;
  orderNumber?: string;
  MerchantId?: string;
  merchantId?: string;
  UserId?: string;
  userId?: string;
  CustomerName?: string;
  customerName?: string;
};

export class OrderCompletedConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "chat-order-completed-queue";
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
      {
        noAck: false,
      },
    );

    this.running = true;
    logger.info("Chat order.completed consumer started");
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
      const customerId = payload.UserId ?? payload.userId;
      const messageId =
        message.properties.messageId ??
        payload.EventId ??
        payload.eventId ??
        orderId ??
        `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !merchantId || !customerId) {
        throw new Error("Invalid order.completed payload");
      }

      const exists = await prisma.chatEventInbox.findUnique({
        where: { messageId },
      });

      if (exists) {
        channel.ack(message);
        return;
      }

      await prisma.$transaction(async (tx) => {
        await tx.chatEventInbox.create({
          data: {
            messageId,
            eventType: "order.completed",
            routingKey: message.fields.routingKey,
            aggregateId: orderId,
            payload: payload as Prisma.InputJsonValue,
            processedAt: new Date(),
          },
        });

        await tx.chatConversation.upsert({
          where: {
            orderId_conversationType: {
              orderId,
              conversationType: "ORDER_MERCHANT",
            },
          },
          create: {
            conversationType: "ORDER_MERCHANT",
            orderId,
            customerId,
            merchantId,
            lastMessagePreview: `Conversation started for order ${orderNumber}`,
          },
          update: {
            customerId,
            merchantId,
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
}
