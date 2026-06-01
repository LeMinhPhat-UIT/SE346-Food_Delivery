import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { logger } from "../../utils/logger";
import { RabbitConsumerMessage, RabbitMqClient } from "../../infrastructure/rabbitmq.client";

type AssignmentAcceptedEventPayload = {
  EventId?: string;
  eventId?: string;
  Data?: Record<string, unknown>;
  data?: Record<string, unknown>;
  OrderId?: string;
  orderId?: string;
  OrderNumber?: string;
  orderNumber?: string;
  CustomerId?: string;
  customerId?: string;
  MerchantId?: string;
  merchantId?: string;
  AcceptedByShipperId?: string;
  acceptedByShipperId?: string;
  AssignmentId?: string;
  assignmentId?: string;
};

export class AssignmentAcceptedConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private readonly queueName = "chat-assignment-accepted-queue";
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    const channel = await this.rabbitMqClient.createConsumerQueue(this.queueName, ["assignment.accepted"]);

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
    logger.info("Chat assignment.accepted consumer started");
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
      const rawPayload = JSON.parse(message.content.toString("utf8")) as AssignmentAcceptedEventPayload;
      const payload = this.unwrapPayload(rawPayload);
      const orderId = payload.OrderId ?? payload.orderId;
      const orderNumber = payload.OrderNumber ?? payload.orderNumber;
      const merchantId = payload.MerchantId ?? payload.merchantId;
      const customerId = payload.CustomerId ?? payload.customerId;
      const shipperId = payload.AcceptedByShipperId ?? payload.acceptedByShipperId;
      const messageId =
        message.properties.messageId ??
        payload.EventId ??
        payload.eventId ??
        orderId ??
        `${message.fields.routingKey}:${message.fields.deliveryTag}`;

      if (!orderId || !orderNumber || !merchantId || !customerId || !shipperId) {
        throw new Error("Invalid assignment.accepted payload");
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
            eventType: "assignment.accepted",
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
              conversationType: "ORDER_SHIPPER",
            },
          },
          create: {
            conversationType: "ORDER_SHIPPER",
            orderId,
            customerId,
            merchantId,
            shipperId,
            lastMessagePreview: `Shipper assigned for order ${orderNumber}`,
          },
          update: {
            customerId,
            merchantId,
            shipperId,
          },
        });
      });

      channel.ack(message);
    } catch (error) {
      logger.error("Failed to process assignment.accepted event", error);
      channel.nack(message, false, false);
    }
  }

  private unwrapPayload(payload: AssignmentAcceptedEventPayload) {
    const data = payload.Data ?? payload.data;

    if (data && typeof data === "object" && !Array.isArray(data)) {
      return {
        ...payload,
        ...data,
        Data: data,
      } as AssignmentAcceptedEventPayload & Record<string, unknown>;
    }

    return payload;
  }
}
