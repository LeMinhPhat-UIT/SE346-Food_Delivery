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
