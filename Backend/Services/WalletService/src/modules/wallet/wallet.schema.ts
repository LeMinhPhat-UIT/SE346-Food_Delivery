import { z } from "zod";

export const ownerTypeParamSchema = z.object({
  ownerType: z.enum(["MERCHANT", "SHIPPER", "ADMIN"]),
});

export const ownerParamSchema = z.object({
  ownerType: z.enum(["MERCHANT", "SHIPPER", "ADMIN"]),
  ownerId: z.string().uuid("Owner id must be a valid UUID"),
});

export const listTransactionsQuerySchema = z.object({
  page: z.coerce.number().int().positive().default(1),
  limit: z.coerce.number().int().positive().max(100).default(20),
});

export const topupQuerySchema = listTransactionsQuerySchema;

export const referenceTransactionParamSchema = z.object({
  referenceType: z.string().trim().min(1).max(50),
  referenceId: z.string().uuid("Reference id must be a valid UUID"),
});

export const orderTransactionParamSchema = z.object({
  orderId: z.string().uuid("Order id must be a valid UUID"),
});

export const negativeWalletsQuerySchema = listTransactionsQuerySchema;

export const topupBodySchema = z
  .object({
    amount: z.coerce.number().positive("Topup amount must be greater than 0"),
    bankCode: z.string().trim().min(1).max(20).optional(),
  })
  .strict();

export const topupParamSchema = z.object({
  topupId: z.string().uuid("Topup id must be a valid UUID"),
});
