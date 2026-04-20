import { z } from "zod";

export const CategorySchema = z.object({
    name: z.string()
        .min(1, "Category name is required")
        .max(100, "Category name must be less than 100 characters")
        .trim(),
    description: z.string()
        .max(500, "Description must be less than 500 characters")
        .nullable()
        .optional(),
    iconUrl: z.string()
        .url("Icon URL must be a valid URL")
        .max(500)
        .nullable()
        .optional(),
    parentId: z.string().uuid("Parent ID must be a valid UUID").nullable().optional(),
    sortOrder: z.number().int().default(0),
    isActive: z.boolean().default(true),
});

// Type for creating a new category 
export type CategoryInput = z.infer<typeof CategorySchema>;

// All fields are optional for updates
export const UpdateCategorySchema = CategorySchema.partial(); 