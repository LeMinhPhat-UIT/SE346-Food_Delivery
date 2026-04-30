import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { uploadMiddleware } from "../../middlewares/upload.middleware";
import { UploadController } from "./upload.controller";
import { deleteUploadSchema, uploadSchema } from "./upload.schema";

const router = Router();
const uploadController = new UploadController();

router.post(
  "/",
  uploadMiddleware.array("files", 10),
  validate(uploadSchema),
  uploadController.uploadFiles
);

router.delete(
  "/",
  validate(deleteUploadSchema),
  uploadController.deleteFiles
);

export default router;
