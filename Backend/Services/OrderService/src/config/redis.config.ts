import { createClient } from "redis";
import { env } from "./env.config";
import { logger } from "../utils/logger";

const redisUrl = env.REDIS_PASSWORD
  ? `redis://:${encodeURIComponent(env.REDIS_PASSWORD)}@${env.REDIS_HOST}:${env.REDIS_PORT}`
  : `redis://${env.REDIS_HOST}:${env.REDIS_PORT}`;

export const redis = createClient({
  url: redisUrl,
});

redis.on("error", (error) => {
  logger.error("Redis client error", error);
});

export const connectRedis = async () => {
  if (!redis.isOpen) {
    await redis.connect();
    logger.info("Connected to Redis");
  }
};

export const disconnectRedis = async () => {
  if (redis.isOpen) {
    await redis.quit();
    logger.info("Disconnected from Redis");
  }
};
