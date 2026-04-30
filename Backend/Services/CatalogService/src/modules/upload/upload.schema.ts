import { z } from "zod";

export const uploadBodySchema = z.object({
  entityType: z.enum(["category", "product", "review"], {
    error: "entityType must be category, product, or review",
  }),
  entityId: z.string().uuid("entityId must be a valid UUID").optional(),
});

export const deleteUploadBodySchema = z.object({
  paths: z
    .array(z.string().trim().min(1, "File path is required"))
    .min(1, "At least one file path is required")
    .max(20, "You can delete at most 20 files at a time"),
});

export const uploadSchema = {
  body: uploadBodySchema,
};

export const deleteUploadSchema = {
  body: deleteUploadBodySchema,
};
