import { z } from "zod";

export const reviewIdParamSchema = z.object({
  id: z.string().uuid("Review id must be a valid UUID"),
});

export const reviewQuerySchema = z.object({
  productId: z.string().uuid("Product ID must be a valid UUID").optional(),
  userId: z.string().uuid("User ID must be a valid UUID").optional(),
  orderId: z.string().uuid("Order ID must be a valid UUID").optional(),
  merchantId: z.string().uuid("Merchant ID must be a valid UUID").optional(),
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(10),
});

export const baseReviewBodySchema = z.object({
  userId: z.string().uuid("User ID must be a valid UUID"),
  orderId: z.string().uuid("Order ID must be a valid UUID"),
  merchantId: z
    .string()
    .uuid("Merchant ID must be a valid UUID")
    .nullable()
    .optional(),
  productId: z
    .string()
    .uuid("Product ID must be a valid UUID")
    .nullable()
    .optional(),
  shipperId: z
    .string()
    .uuid("Shipper ID must be a valid UUID")
    .nullable()
    .optional(),
  rating: z
    .number({ error: "Rating must be a number" })
    .int("Rating must be an integer")
    .min(1, "Rating must be between 1 and 5")
    .max(5, "Rating must be between 1 and 5"),
  comment: z
    .string()
    .trim()
    .max(2000, "Comment must be at most 2000 characters")
    .nullable()
    .optional(),
  images: z
    .array(z.string().trim().url("Each image must be a valid URL"))
    .max(5, "Images can contain at most 5 items")
    .nullable()
    .optional(),
  merchantReply: z
    .string()
    .trim()
    .max(2000, "Merchant reply must be at most 2000 characters")
    .nullable()
    .optional(),
  repliedAt: z.coerce.date().nullable().optional(),
});

export const createReviewBodySchema = baseReviewBodySchema;

export const updateReviewBodySchema = baseReviewBodySchema
  .partial()
  .refine((data) => Object.keys(data).length > 0, {
    message: "At least one field is required for update",
  });

export const createReviewSchema = {
  body: createReviewBodySchema,
};

export const updateReviewSchema = {
  params: reviewIdParamSchema,
  body: updateReviewBodySchema,
};

export const reviewIdSchema = {
  params: reviewIdParamSchema,
};

export const listReviewsSchema = {
  query: reviewQuerySchema,
};
