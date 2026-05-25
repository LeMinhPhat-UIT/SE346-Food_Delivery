import app from "./app";
import { connectDatabase, disconnectDatabase } from "./config/db.config";
import { env } from "./config/env.config";
import { logger } from "./utils/logger";
import { OrderCompletedConsumer } from "./modules/events/order-completed.consumer";

const startServer = async () => {
  try {
    await connectDatabase();
    const orderCompletedConsumer = new OrderCompletedConsumer();
    await orderCompletedConsumer.start();

    const server = app.listen(env.PORT, () => {
      logger.info(`Wallet Service is running on port ${env.PORT}`);
    });

    const shutdown = async (signal: string) => {
      logger.warn(`Received ${signal}. Shutting down Wallet Service...`);

      server.close(async () => {
        await orderCompletedConsumer.stop();
        await disconnectDatabase();
        process.exit(0);
      });
    };

    process.on("SIGINT", () => void shutdown("SIGINT"));
    process.on("SIGTERM", () => void shutdown("SIGTERM"));
  } catch (error) {
    logger.error("Failed to start Wallet Service", error);
    process.exit(1);
  }
};

void startServer();
