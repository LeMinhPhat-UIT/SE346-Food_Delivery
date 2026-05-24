import amqplib, { ConsumeMessage } from "amqplib";
import { env } from "../config/env.config";

export class RabbitMqClient {
  private connection: any = null;
  private channel: any = null;
  private connecting: Promise<void> | null = null;

  async init() {
    if (this.channel) {
      return;
    }

    if (!this.connecting) {
      this.connecting = this.connect();
    }

    await this.connecting;
  }

  async publishJson(
    routingKey: string,
    payload: unknown,
    options?: {
      messageId?: string;
      correlationId?: string;
    },
  ) {
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
        messageId: options?.messageId,
        correlationId: options?.correlationId,
      },
    );
  }

  async createConsumerQueue(queueName: string, routingKeys: string[]) {
    await this.init();

    if (!this.channel) {
      throw new Error("RabbitMQ channel is not ready");
    }

    await this.channel.assertQueue(queueName, {
      durable: true,
    });

    for (const routingKey of routingKeys) {
      await this.channel.bindQueue(
        queueName,
        env.RABBITMQ_EXCHANGE,
        routingKey,
      );
    }

    return this.channel;
  }

  async close() {
    try {
      await this.channel?.close?.();
    } catch {
      // ignore close errors
    }

    try {
      await this.connection?.close?.();
    } catch {
      // ignore close errors
    }
    this.channel = null;
    this.connection = null;
    this.connecting = null;
  }

  private async connect() {
    this.connection = await amqplib.connect(env.RABBITMQ_URL);
    this.connection.on("close", () => {
      this.channel = null;
      this.connection = null;
      this.connecting = null;
    });
    this.connection.on("error", () => {
      this.channel = null;
      this.connection = null;
      this.connecting = null;
    });

    this.channel = await this.connection.createChannel();
    await this.channel.assertExchange(env.RABBITMQ_EXCHANGE, "topic", {
      durable: true,
    });
  }
}

export type RabbitConsumerMessage = ConsumeMessage;
