import { z } from "zod";
import { uploadBodySchema } from "./upload.schema";

export type UploadBodyDto = z.infer<typeof uploadBodySchema>;

export type UploadFileResponseDto = {
  bucket: string;
  entityType: "category" | "product" | "review";
  entityId: string | null;
  fileName: string;
  originalName: string;
  mimeType: string;
  size: number;
  path: string;
  publicUrl: string;
};
