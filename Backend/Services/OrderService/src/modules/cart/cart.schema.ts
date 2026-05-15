import { z } from "zod";

const cartItemIdParamSchema = z.object({
  itemId: z.string().uuid("Cart item id must be a valid UUID"),
});

const merchantIdParamSchema = z.object({
  merchantId: z.string().uuid("Merchant id must be a valid UUID"),
});

const selectedOptionSchema = z.object({
  optionId: z.string().uuid("Option id must be a valid UUID"),
  valueIds: z
    .array(z.string().uuid("Option value id must be a valid UUID"))
    .min(1, "At least one option value is required"),
});

export const addCartItemBodySchema = z.object({
  productId: z.string().uuid("Product id must be a valid UUID"),
  quantity: z.coerce.number().int().min(1).max(100),
  note: z.string().trim().max(500).nullable().optional(),
  selectedOptions: z.array(selectedOptionSchema).default([]),
});

export const updateCartItemBodySchema = z
  .object({
    quantity: z.coerce.number().int().min(1).max(100).optional(),
    note: z.string().trim().max(500).nullable().optional(),
    selectedOptions: z.array(selectedOptionSchema).optional(),
  })
  .refine((data) => Object.keys(data).length > 0, {
    message: "At least one field is required for update",
  });

export const addCartItemSchema = {
  body: addCartItemBodySchema,
};

export const updateCartItemSchema = {
  params: cartItemIdParamSchema,
  body: updateCartItemBodySchema,
};

export const cartItemIdSchema = {
  params: cartItemIdParamSchema,
};

export const merchantCartSchema = {
  params: merchantIdParamSchema,
};
