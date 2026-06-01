import { z } from "zod";

export const chatConversationTypeSchema = z.enum(["ORDER_MERCHANT", "ORDER_SHIPPER"]);
export const chatMessageTypeSchema = z.enum(["TEXT", "IMAGE", "SYSTEM"]);

export const paginationSchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  limit: z.coerce.number().int().min(1).max(100).default(20),
});

export const conversationIdParamSchema = z.object({
  conversationId: z.string().uuid("conversationId must be a valid UUID"),
});

export const conversationOrderParamSchema = z.object({
  orderId: z.string().min(1, "orderId is required"),
  conversationType: chatConversationTypeSchema,
});

export const createConversationSchema = z
  .object({
    conversationType: chatConversationTypeSchema,
    orderId: z.string().min(1, "orderId is required"),
    deliveryId: z.string().min(1).optional(),
    customerId: z.string().min(1, "customerId is required"),
    merchantId: z.string().min(1, "merchantId is required"),
    shipperId: z.string().min(1).optional(),
  })
  .superRefine((value, ctx) => {
    if (value.conversationType === "ORDER_SHIPPER") {
      if (!value.deliveryId) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["deliveryId"],
          message: "deliveryId is required for order shipper conversation",
        });
      }

      if (!value.shipperId) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["shipperId"],
          message: "shipperId is required for order shipper conversation",
        });
      }
    }
  });

export const createMessageSchema = z.object({
  content: z.string().min(1, "content is required").max(4000, "content is too long"),
  messageType: chatMessageTypeSchema.default("TEXT"),
});

export const chatFilterSchema = paginationSchema.extend({
  conversationType: chatConversationTypeSchema.optional(),
});
