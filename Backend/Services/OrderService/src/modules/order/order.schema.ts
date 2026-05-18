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

export const createOrderBodySchema = z.object({
  merchantId: z.string().uuid("Merchant id must be a valid UUID"),
  addressId: z.string().uuid("Address id must be a valid UUID"),
  voucherCode: z
    .string()
    .trim()
    .max(50, "Voucher code must be at most 50 characters")
    .optional(),
  paymentMethod: z.enum(["COD", "VNPAY"]),
  note: z
    .string()
    .trim()
    .max(1000, "Order note must be at most 1000 characters")
    .optional(),
});

export const checkoutPreviewSchema = {
  body: checkoutPreviewBodySchema,
};

export const createOrderSchema = {
  body: createOrderBodySchema,
};

export const orderIdParamSchema = z.object({
  id: z.string().uuid("Order id must be a valid UUID"),
});

export const myOrdersQuerySchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(10),
  merchantId: z.string().uuid("Merchant id must be a valid UUID").optional(),
  status: z
    .enum([
      "PENDING",
      "CONFIRMED",
      "PREPARING",
      "READY",
      "PICKED_UP",
      "DELIVERING",
      "DELIVERED",
      "CANCELLED",
    ])
    .optional(),
  paymentStatus: z
    .enum(["PENDING", "PAID", "FAILED", "REFUNDED"])
    .optional(),
  sortBy: z.enum(["createdAt", "totalAmount"]).default("createdAt"),
  sortOrder: z.enum(["asc", "desc"]).default("desc"),
});

export const myOrdersSchema = {
  query: myOrdersQuerySchema,
};

export const orderIdSchema = {
  params: orderIdParamSchema,
};

export const updateOrderStatusBodySchema = z
  .object({
    status: z.enum([
      "CONFIRMED",
      "PREPARING",
      "READY",
      "CANCELLED",
    ]),
    note: z
      .string()
      .trim()
      .max(1000, "Status note must be at most 1000 characters")
      .optional(),
    cancelReason: z
      .string()
      .trim()
      .max(1000, "Cancel reason must be at most 1000 characters")
      .optional(),
  })
  .refine(
    (data) => data.status !== "CANCELLED" || Boolean(data.cancelReason?.trim()),
    {
      message: "Cancel reason is required when cancelling an order",
      path: ["cancelReason"],
    },
  );

export const cancelOrderBodySchema = z.object({
  cancelReason: z
    .string()
    .trim()
    .min(1, "Cancel reason is required")
    .max(1000, "Cancel reason must be at most 1000 characters"),
});

export const updateOrderStatusSchema = {
  params: orderIdParamSchema,
  body: updateOrderStatusBodySchema,
};

export const cancelOrderSchema = {
  params: orderIdParamSchema,
  body: cancelOrderBodySchema,
};
