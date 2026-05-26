import { prisma } from "../prisma/prisma.client";
import { logger } from "../utils/logger";

export const connectDatabase = async () => {
  await prisma.$connect();
  logger.info("Wallet database connected");
};

export const disconnectDatabase = async () => {
  await prisma.$disconnect();
  logger.info("Wallet database disconnected");
};
