import { DeliveryMilestoneEventPayload } from "./order.events";
import { logger } from "../../utils/logger";
import { RabbitMqClient, RabbitConsumerMessage } from "../../infrastructure/rabbitmq.client";
import { OrderService } from "../order/order.service";

const QUEUE_NAME = "order-service.delivery-milestone-queue";
const ROUTING_KEYS = ["delivery.milestone"];

export class DeliveryMilestoneConsumer {
  private readonly rabbitMqClient = new RabbitMqClient();
  private consuming = false;
  private retryTimer: NodeJS.Timeout | null = null;

  constructor(private readonly orderService: OrderService) {}

  async start() {
    if (this.consuming) {
      return;
    }

    this.consuming = true;
    void this.tryStartConsuming();
  }

  async stop() {
    this.consuming = false;
    if (this.retryTimer) {
      clearTimeout(this.retryTimer);
      this.retryTimer = null;
    }
    await this.rabbitMqClient.close();
  }

  private async tryStartConsuming() {
    if (!this.consuming) {
      return;
    }

    try {
      const channel = await this.rabbitMqClient.createConsumerQueue(
        QUEUE_NAME,
        ROUTING_KEYS,
      );

      await channel.consume(QUEUE_NAME, async (message: RabbitConsumerMessage | null) => {
        if (!message) {
          return;
        }

        try {
          const payload = JSON.parse(
            message.content.toString("utf8"),
          ) as DeliveryMilestoneEventPayload;
          await this.orderService.handleDeliveryMilestone(payload);
          channel.ack(message);
        } catch (error) {
          logger.error("Failed to process delivery milestone event", error);
          channel.nack(message, false, false);
        }
      });
    } catch (error) {
      logger.warn("RabbitMQ is not ready for delivery milestone consuming yet", error);
      this.retryTimer = setTimeout(() => {
        void this.tryStartConsuming();
      }, 5000);
    }
  }
}

export type DeliveryMilestoneConsumerMessage = RabbitConsumerMessage;
