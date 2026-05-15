import app from "./app";
import { connectDatabase, disconnectDatabase } from "./config/db.config";
import { env } from "./config/env.config";
import { connectRedis, disconnectRedis } from "./config/redis.config";
import { logger } from "./utils/logger";

const startServer = async () => {
  try {
    await connectDatabase();
    await connectRedis();

    const server = app.listen(env.PORT, () => {
      logger.info(`Order Service is running on port ${env.PORT}`);
    });

    const shutdown = async (signal: string) => {
      logger.warn(`Received ${signal}. Shutting down Order Service...`);

      server.close(async () => {
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
