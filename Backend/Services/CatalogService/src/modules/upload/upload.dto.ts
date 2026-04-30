import { z } from "zod";
import { deleteUploadBodySchema, uploadBodySchema } from "./upload.schema";

export type UploadBodyDto = z.infer<typeof uploadBodySchema>;
export type DeleteUploadDto = z.infer<typeof deleteUploadBodySchema>;

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

export type DeleteUploadResponseDto = {
  bucket: string;
  deletedPaths: string[];
};
