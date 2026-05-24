import { z } from "zod";

export const orderIdParamSchema = z.object({
  orderId: z.string().uuid("Order id must be a valid UUID"),
});

export const createVnpayPaymentUrlBodySchema = z
  .object({
    bankCode: z.string().trim().min(1).max(20).optional(),
  })
  .strict();

