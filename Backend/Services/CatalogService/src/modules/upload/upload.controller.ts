import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { UploadBodyDto } from "./upload.dto";
import { UploadService } from "./upload.service";

const uploadService = new UploadService();

export class UploadController {
  uploadFiles = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as UploadBodyDto;
    const files = (req.files as Express.Multer.File[]) ?? [];
    const uploadedFiles = await uploadService.uploadFiles(payload, files);

    return Send.success(
      res,
      uploadedFiles,
      "Files uploaded successfully",
      HTTP_STATUS.CREATED
    );
  });
}
