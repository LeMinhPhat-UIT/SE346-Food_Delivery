import amqp from "amqplib";
import { env } from "../config/env.config";
import { logger } from "../utils/logger";

export type RabbitConsumerMessage = any;

export class RabbitMqClient {
  private connection: any = null;
  private channel: any = null;
  private connecting: Promise<void> | null = null;

  async init() {
    if (this.channel) {
      return;
    }

    if (this.connecting) {
      return this.connecting;
    }

    this.connecting = (async () => {
      if (!this.connection) {
        this.connection = await amqp.connect(env.RABBITMQ_URL);
        this.connection.on("close", () => {
          logger.warn("RabbitMQ connection closed");
          this.connection = null;
          this.channel = null;
        });
        this.connection.on("error", (error: unknown) => {
          logger.error("RabbitMQ connection error", error);
        });
      }

      if (!this.channel) {
        this.channel = await this.connection.createChannel();
        await this.channel.assertExchange(env.RABBITMQ_EXCHANGE, "topic", { durable: true });
      }
    })();

    try {
      await this.connecting;
    } finally {
      this.connecting = null;
    }
  }

  async createConsumerQueue(queueName: string, routingKeys: string[]) {
    await this.init();

    if (!this.channel) {
      throw new Error("RabbitMQ channel is not ready");
    }

    await this.channel.assertQueue(queueName, { durable: true });

    for (const routingKey of routingKeys) {
      await this.channel.bindQueue(queueName, env.RABBITMQ_EXCHANGE, routingKey);
    }

    return this.channel;
  }

  async publishJson(routingKey: string, payload: unknown) {
    await this.init();

    if (!this.channel) {
      throw new Error("RabbitMQ channel is not ready");
    }

    this.channel.publish(
      env.RABBITMQ_EXCHANGE,
      routingKey,
      Buffer.from(JSON.stringify(payload)),
      {
        contentType: "application/json",
        persistent: true,
      },
    );
  }

  async close() {
    if (this.channel) {
      await this.channel.close();
      this.channel = null;
    }

    if (this.connection) {
      await this.connection.close();
      this.connection = null;
    }
  }
}
