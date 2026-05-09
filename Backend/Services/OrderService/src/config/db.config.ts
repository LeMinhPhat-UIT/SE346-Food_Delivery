import { prisma } from "../prisma/prisma.client";
import { logger } from "../utils/logger";

export const connectDatabase = async () => {
  await prisma.$connect();
  logger.info("Connected to database");
};

export const disconnectDatabase = async () => {
  await prisma.$disconnect();
  logger.info("Disconnected from database");
};
