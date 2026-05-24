import app from "./app";
import { connectDatabase, disconnectDatabase } from "./config/db.config";
import { env } from "./config/env.config";
import { connectRedis, disconnectRedis } from "./config/redis.config";
import { DeliveryMilestoneConsumer } from "./modules/events/delivery-milestone.consumer";
import { OutboxDispatcher } from "./modules/outbox/outbox.dispatcher";
import { orderService } from "./modules/order/order.bootstrap";
import { logger } from "./utils/logger";

const startServer = async () => {
  try {
    await connectDatabase();
    await connectRedis();

    const outboxDispatcher = new OutboxDispatcher();
    const deliveryMilestoneConsumer = new DeliveryMilestoneConsumer(orderService);

    await outboxDispatcher.start();
    await deliveryMilestoneConsumer.start();

    const server = app.listen(env.PORT, () => {
      logger.info(`Order Service is running on port ${env.PORT}`);
    });

    const shutdown = async (signal: string) => {
      logger.warn(`Received ${signal}. Shutting down Order Service...`);

      server.close(async () => {
        await deliveryMilestoneConsumer.stop();
        await outboxDispatcher.stop();
        await disconnectRedis();
        await disconnectDatabase();
        process.exit(0);
      });
    };

    process.on("SIGINT", () => void shutdown("SIGINT"));
    process.on("SIGTERM", () => void shutdown("SIGTERM"));
  } catch (error) {
    logger.error("Failed to start Order Service", error);
    process.exit(1);
  }
};

void startServer();
