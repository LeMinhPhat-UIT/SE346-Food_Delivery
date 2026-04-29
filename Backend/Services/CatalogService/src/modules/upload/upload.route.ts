import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { uploadMiddleware } from "../../middlewares/upload.middleware";
import { UploadController } from "./upload.controller";
import { uploadSchema } from "./upload.schema";

const router = Router();
const uploadController = new UploadController();

router.post(
  "/",
  uploadMiddleware.array("files", 10),
  validate(uploadSchema),
  uploadController.uploadFiles
);

export default router;
