import path from "node:path";
import { v4 as uuidv4 } from "uuid";
import { env } from "../../config/env.config";
import { supabase } from "../../config/supabase.config";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { logger } from "../../utils/logger";
import {
  DeleteUploadDto,
  DeleteUploadResponseDto,
  UploadBodyDto,
  UploadFileResponseDto,
} from "./upload.dto";

export class UploadService {
  async uploadFiles(
    payload: UploadBodyDto,
    files: Express.Multer.File[]
  ): Promise<UploadFileResponseDto[]> {
    if (!files.length) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "At least one file is required");
    }

    const uploadedFiles = await Promise.all(
      files.map(async (file) => {
        const fileExtension = this.getFileExtension(file);
        const fileName = `${uuidv4()}${fileExtension}`;
        const entityFolder = payload.entityId ?? "general";
        const filePath = `${payload.entityType}/${entityFolder}/${fileName}`;

        const { error } = await supabase.storage
          .from(env.SUPABASE_STORAGE_BUCKET)
          .upload(filePath, file.buffer, {
            contentType: file.mimetype,
            upsert: false,
          });

        if (error) {
          throw new ApiError(
            HTTP_STATUS.INTERNAL_SERVER_ERROR,
            `Failed to upload file to Supabase Storage: ${error.message}`
          );
        }

        const { data: publicUrlData } = supabase.storage
          .from(env.SUPABASE_STORAGE_BUCKET)
          .getPublicUrl(filePath);

        return {
          bucket: env.SUPABASE_STORAGE_BUCKET,
          entityType: payload.entityType,
          entityId: payload.entityId ?? null,
          fileName,
          originalName: file.originalname,
          mimeType: file.mimetype,
          size: file.size,
          path: filePath,
          publicUrl: publicUrlData.publicUrl,
        };
      })
    );

    return uploadedFiles;
  }

  async deleteFiles(payload: DeleteUploadDto): Promise<DeleteUploadResponseDto> {
    const normalizedPaths = payload.paths.map((filePath) => filePath.trim());

    const { data, error } = await supabase.storage
      .from(env.SUPABASE_STORAGE_BUCKET)
      .remove(normalizedPaths);

    if (error) {
      throw new ApiError(
        HTTP_STATUS.INTERNAL_SERVER_ERROR,
        `Failed to delete file from Supabase Storage: ${error.message}`
      );
    }

    return {
      bucket: env.SUPABASE_STORAGE_BUCKET,
      deletedPaths: (data ?? []).map((item) => item.name),
    };
  }

  async deleteFilesByPublicUrls(urls: string[]) {
    const normalizedPaths = urls
      .map((url) => this.extractStoragePath(url))
      .filter((filePath): filePath is string => Boolean(filePath));

    if (!normalizedPaths.length) {
      return;
    }

    try {
      await this.deleteFiles({
        paths: normalizedPaths,
      });
    } catch (error) {
      logger.warn("Failed to clean up old files from storage", {
        paths: normalizedPaths,
        error,
      });
    }
  }

  private getFileExtension(file: Express.Multer.File) {
    const originalExtension = path.extname(file.originalname).toLowerCase();

    if (originalExtension) {
      return originalExtension;
    }

    switch (file.mimetype) {
      case "image/png":
        return ".png";
      case "image/webp":
        return ".webp";
      default:
        return ".jpg";
    }
  }

  private extractStoragePath(value: string) {
    const trimmedValue = value.trim();

    if (!trimmedValue) {
      return null;
    }

    const publicPrefix = `${env.SUPABASE_PROJECT_URL}/storage/v1/object/public/${env.SUPABASE_STORAGE_BUCKET}/`;

    if (trimmedValue.startsWith(publicPrefix)) {
      return decodeURIComponent(trimmedValue.slice(publicPrefix.length));
    }

    const rawPathPrefix = `${env.SUPABASE_STORAGE_BUCKET}/`;

    if (trimmedValue.startsWith(rawPathPrefix)) {
      return trimmedValue.slice(rawPathPrefix.length);
    }

    if (
      trimmedValue.startsWith("category/") ||
      trimmedValue.startsWith("product/") ||
      trimmedValue.startsWith("review/")
    ) {
      return trimmedValue;
    }

    return null;
  }
}
