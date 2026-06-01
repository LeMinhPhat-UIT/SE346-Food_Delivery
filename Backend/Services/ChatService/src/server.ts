import app from "./app";
import { env } from "./config/env.config";
import { logger } from "./utils/logger";
import {
  assignmentAcceptedConsumer,
  orderCompletedConsumer,
} from "./modules/events/chat.bootstrap";

const port = env.PORT;

const server = app.listen(port, async () => {
  logger.info(`Chat Service listening on port ${port}`);

  try {
    await orderCompletedConsumer.start();
    await assignmentAcceptedConsumer.start();
  } catch (error) {
    logger.error("Failed to start chat consumers", error);
  }
});

const shutdown = async (signal: string) => {
  logger.info(`Chat Service received ${signal}, shutting down`);
  server.close();
  await orderCompletedConsumer.stop();
  await assignmentAcceptedConsumer.stop();
  process.exit(0);
};

process.on("SIGINT", () => {
  void shutdown("SIGINT");
});

process.on("SIGTERM", () => {
  void shutdown("SIGTERM");
});
