import { z } from "zod";

export const productIdParamSchema = z.object({
  id: z.string().uuid("Product id must be a valid UUID"),
});

export const productOptionValueSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Option value name is required")
    .max(100, "Option value name must be at most 100 characters"),
  additionalPrice: z
    .number({ error: "Additional price must be a number" })
    .min(0, "Additional price must be greater than or equal to 0")
    .default(0),
  isAvailable: z.boolean().default(true),
});

export const productOptionSchema = z.object({
  categoryId: z
    .string()
    .uuid("Category ID must be a valid UUID")
    .nullable()
    .optional(),
  name: z
    .string()
    .trim()
    .min(1, "Product name is required")
    .max(100, "Product name must be at most 100 characters"),
  isRequired: z.boolean().default(false),
  maxSelections: z
    .number({ error: "Max selection must be a number" })
    .int("Max selection must be an integer")
    .min(1, "Max selection must be at least 1")
    .default(1),
  values: z.array(productOptionValueSchema).default([]),
});

export const baseProductBodySchema = z.object({
  merchantId: z.string().uuid("Merchant ID must be a valid UUID"),
  categoryId: z
    .string()
    .uuid("Category ID must be a valid UUID")
    .nullable()
    .optional(),
  name: z
    .string()
    .trim()
    .min(1, "Product name is required")
    .max(255, "Product name must be at most 255 characters"),
  description: z
    .string()
    .trim()
    .max(1000, "Description must be at most 1000 characters")
    .nullable()
    .optional(),
  imageUrl: z
    .string()
    .trim()
    .url("Image URL must be a valid URL")
    .max(500, "Image URL must be at most 500 characters")
    .nullable()
    .optional(),
  basePrice: z
    .number({ error: "Base price must be a number" })
    .positive("Base price must be greater than 0"),
  discountPrice: z
    .number({ error: "Discount price must be a number" })
    .min(0, "Discount price must be greater than or equal to 0")
    .nullable()
    .optional(),
  isAvailable: z.boolean().default(true),
  isFeatured: z.boolean().default(false),
  prepTime: z
    .number({ error: "Prep time must be a number" })
    .int("Prep time must be an integer")
    .min(0, "Prep time must be greater than or equal to 0")
    .nullable()
    .optional(),
  options: z.array(productOptionSchema).default([]),
});

export const createProductBodySchema = baseProductBodySchema.refine(
  (data) =>
    data.discountPrice === undefined ||
    data.discountPrice === null ||
    data.discountPrice <= data.basePrice,
  {
    message: "Discount price cannot be greater than base price",
    path: ["discountPrice"],
  },
);

export const updateProductBodySchema = baseProductBodySchema
  .partial()
  .refine((data) => Object.keys(data).length > 0, {
    message: "At least one field is required for update",
  })
  .refine(
    (data) =>
      data.basePrice === undefined ||
      data.discountPrice === undefined ||
      data.discountPrice === null ||
      data.discountPrice <= data.basePrice,
    {
      message: "Discount price cannot be greater than base price",
      path: ["discountPrice"],
    },
  );

export const createProductSchema = {
  body: createProductBodySchema,
};

export const updateProductSchema = {
  params: productIdParamSchema,
  body: updateProductBodySchema,
};

export const productIdSchema = {
  params: productIdParamSchema,
};
