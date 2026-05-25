import { z } from "zod";

export const dateRangeQuerySchema = z.object({
  from: z.string().trim().optional(),
  to: z.string().trim().optional(),
});
