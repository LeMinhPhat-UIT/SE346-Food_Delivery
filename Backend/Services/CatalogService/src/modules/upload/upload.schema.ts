import { z } from "zod";

export const uploadBodySchema = z.object({
  entityType: z.enum(["category", "product", "review"], {
    error: "entityType must be category, product, or review",
  }),
  entityId: z.string().uuid("entityId must be a valid UUID").optional(),
});

export const uploadSchema = {
  body: uploadBodySchema,
};
