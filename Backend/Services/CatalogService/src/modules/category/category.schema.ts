import { z } from "zod";

export const categoryIdParamSchema = z.object({
  id: z.string().uuid("Category id must be a valid UUID"),
});

export const categoryQuerySchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(10),
  search: z.string().trim().optional(),
  parentId: z.string().uuid("Parent ID must be a valid UUID").optional(),
  isActive: z.coerce.boolean().optional(),
  includeDeleted: z.coerce.boolean().default(false),
  sortBy: z
    .enum(["name", "sortOrder", "createdAt", "productCount"])
    .default("sortOrder"),
  sortOrder: z.enum(["asc", "desc"]).default("asc"),
});

export const createCategoryBodySchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Category name is required")
    .max(100, "Category name must be at most 100 characters"),
  description: z
    .string()
    .trim()
    .max(500, "Description must be at most 500 characters")
    .nullable()
    .optional(),
  iconUrl: z
    .string()
    .trim()
    .url("Icon URL must be a valid URL")
    .max(500, "Icon URL must be at most 500 characters")
    .nullable()
    .optional(),
  parentId: z
    .string()
    .uuid("Parent ID must be a valid UUID")
    .nullable()
    .optional(),
  sortOrder: z.number().int("Sort order must be an integer").default(0),
  isActive: z.boolean().default(true),
});

export const updateCategoryBodySchema = createCategoryBodySchema
  .partial()
  .refine((data) => Object.keys(data).length > 0, {
    message: "At least one field is required for update",
  });

export const updateCategoryStatusBodySchema = z.object({
  isActive: z.boolean(),
});

export const createCategorySchema = {
  body: createCategoryBodySchema,
};

export const listCategoriesSchema = {
  query: categoryQuerySchema,
};

export const updateCategorySchema = {
  params: categoryIdParamSchema,
  body: updateCategoryBodySchema,
};

export const categoryIdSchema = {
  params: categoryIdParamSchema,
};

export const updateCategoryStatusSchema = {
  params: categoryIdParamSchema,
  body: updateCategoryStatusBodySchema,
};

export const restoreCategorySchema = {
  params: categoryIdParamSchema,
};
