import { z } from "zod";

const voucherIdParamSchema = z.object({
  id: z.string().uuid("Voucher id must be a valid UUID"),
});

const voucherCodeParamSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, "Voucher code is required")
    .max(50, "Voucher code must be at most 50 characters"),
});

export const voucherQuerySchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(10),
  search: z.string().trim().optional(),
  merchantId: z.string().uuid("Merchant id must be a valid UUID").optional(),
  isActive: z.coerce.boolean().optional(),
  includeDeleted: z.coerce.boolean().default(false),
  discountType: z.enum(["PERCENTAGE", "FIXED"]).optional(),
  discountTarget: z.enum(["SUBTOTAL", "DELIVERY_FEE"]).optional(),
  availability: z.enum(["active", "upcoming", "expired", "inactive"]).optional(),
  sortBy: z
    .enum([
      "createdAt",
      "startDate",
      "endDate",
      "code",
      "name",
      "discountValue",
      "usedCount",
    ])
    .default("createdAt"),
  sortOrder: z.enum(["asc", "desc"]).default("desc"),
});

const voucherBodyBaseSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, "Voucher code is required")
    .max(50, "Voucher code must be at most 50 characters"),
  name: z
    .string()
    .trim()
    .min(1, "Voucher name is required")
    .max(255, "Voucher name must be at most 255 characters"),
  description: z
    .string()
    .trim()
    .max(2000, "Description must be at most 2000 characters")
    .nullable()
    .optional(),
  discountType: z.enum(["PERCENTAGE", "FIXED"]),
  discountValue: z.coerce
    .number()
    .positive("Discount value must be greater than 0"),
  maxDiscount: z.coerce
    .number()
    .positive("Max discount must be greater than 0")
    .nullable()
    .optional(),
  minOrderAmount: z.coerce
    .number()
    .min(0, "Minimum order amount must be at least 0")
    .nullable()
    .optional(),
  discountTarget: z.enum(["SUBTOTAL", "DELIVERY_FEE"]).default("SUBTOTAL"),
  merchantId: z
    .string()
    .uuid("Merchant id must be a valid UUID")
    .nullable()
    .optional(),
  usageLimit: z.coerce
    .number()
    .int("Usage limit must be an integer")
    .positive("Usage limit must be greater than 0")
    .nullable()
    .optional(),
  perUserLimit: z.coerce
    .number()
    .int("Per-user limit must be an integer")
    .positive("Per-user limit must be greater than 0")
    .default(1),
  startDate: z.coerce.date({
    error: "Start date must be a valid ISO date",
  }),
  endDate: z.coerce.date({
    error: "End date must be a valid ISO date",
  }),
  isActive: z.boolean().default(true),
});

export const createVoucherBodySchema = voucherBodyBaseSchema
  .refine(
    (data) =>
      !(data.discountType === "PERCENTAGE" && data.discountValue > 100),
    {
      message: "Percentage discount value must not exceed 100",
      path: ["discountValue"],
    },
  )
  .refine(
    (data) => !(data.discountType === "FIXED" && data.maxDiscount),
    {
      message: "Max discount should only be used for percentage vouchers",
      path: ["maxDiscount"],
    },
  )
  .refine((data) => data.endDate > data.startDate, {
    message: "End date must be later than start date",
    path: ["endDate"],
  });

export const updateVoucherBodySchema = voucherBodyBaseSchema
  .partial()
  .refine((data) => Object.keys(data).length > 0, {
  message: "At least one field is required for update",
})
  .refine(
    (data) =>
      !(
        data.discountType === "PERCENTAGE" &&
        data.discountValue !== undefined &&
        data.discountValue > 100
      ),
    {
      message: "Percentage discount value must not exceed 100",
      path: ["discountValue"],
    },
  )
  .refine(
    (data) => !(data.discountType === "FIXED" && data.maxDiscount),
    {
      message: "Max discount should only be used for percentage vouchers",
      path: ["maxDiscount"],
    },
  )
  .refine(
    (data) =>
      !(
        data.startDate !== undefined &&
        data.endDate !== undefined &&
        data.endDate <= data.startDate
      ),
    {
      message: "End date must be later than start date",
      path: ["endDate"],
    },
  );

export const updateVoucherStatusBodySchema = z.object({
  isActive: z.boolean(),
});

export const validateVoucherBodySchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, "Voucher code is required")
    .max(50, "Voucher code must be at most 50 characters"),
  userId: z.string().uuid("User id must be a valid UUID"),
  merchantId: z.string().uuid("Merchant id must be a valid UUID").optional(),
  subtotal: z.coerce
    .number()
    .min(0, "Subtotal must be at least 0"),
  deliveryFee: z.coerce
    .number()
    .min(0, "Delivery fee must be at least 0")
    .default(0),
});

export const listVouchersSchema = {
  query: voucherQuerySchema,
};

export const voucherIdSchema = {
  params: voucherIdParamSchema,
};

export const voucherCodeSchema = {
  params: voucherCodeParamSchema,
};

export const createVoucherSchema = {
  body: createVoucherBodySchema,
};

export const updateVoucherSchema = {
  params: voucherIdParamSchema,
  body: updateVoucherBodySchema,
};

export const updateVoucherStatusSchema = {
  params: voucherIdParamSchema,
  body: updateVoucherStatusBodySchema,
};

export const restoreVoucherSchema = {
  params: voucherIdParamSchema,
};

export const validateVoucherSchema = {
  body: validateVoucherBodySchema,
};
