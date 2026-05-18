import { z } from "zod";

export const checkoutPreviewBodySchema = z.object({
  merchantId: z.string().uuid("Merchant id must be a valid UUID"),
  addressId: z.string().uuid("Address id must be a valid UUID"),
  voucherCode: z
    .string()
    .trim()
    .max(50, "Voucher code must be at most 50 characters")
    .optional(),
  paymentMethod: z.enum(["COD", "VNPAY"]).optional(),
});

export const checkoutPreviewSchema = {
  body: checkoutPreviewBodySchema,
};
