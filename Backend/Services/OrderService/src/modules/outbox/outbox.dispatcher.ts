import { env } from "../../config/env.config";
import { logger } from "../../utils/logger";
import { OutboxRepository } from "./outbox.repository";
import { RabbitMqClient } from "../../infrastructure/rabbitmq.client";

export class OutboxDispatcher {
  private readonly outboxRepository = new OutboxRepository();
  private readonly rabbitMqClient = new RabbitMqClient();
  private timer: NodeJS.Timeout | null = null;
  private running = false;

  async start() {
    if (this.running) {
      return;
    }

    this.running = true;
    this.timer = setInterval(() => {
      void this.flushPendingMessages();
    }, env.OUTBOX_POLL_INTERVAL_MS);

    void this.flushPendingMessages();
  }

  async stop() {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }

    this.running = false;
    await this.rabbitMqClient.close();
  }

  private async flushPendingMessages() {
    try {
      await this.rabbitMqClient.init();
    } catch (error) {
      logger.warn("RabbitMQ is not ready for outbox publishing yet", error);
      return;
    }

    const messages = await this.outboxRepository.findPending(env.OUTBOX_BATCH_SIZE);

    for (const message of messages) {
      try {
        await this.rabbitMqClient.publishJson(message.eventType, message.payload, {
          messageId: message.id,
          correlationId: message.aggregateId,
        });

        await this.outboxRepository.markPublished(message.id);
      } catch (error) {
        logger.error("Failed to publish outbox message", error);
        return;
      }
    }
  }
}
