import { ROLES } from "../../constants/roles";
import {
  attachMerchantContext,
  authenticate,
  requireRoles,
} from "../../middlewares/auth.middleware";
import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { uploadMiddleware } from "../../middlewares/upload.middleware";
import { UploadController } from "./upload.controller";
import { deleteUploadSchema, uploadSchema } from "./upload.schema";

const router = Router();
const uploadController = new UploadController();

router.post(
  "/",
  authenticate,
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.CUSTOMER),
  uploadMiddleware.array("files", 10),
  validate(uploadSchema),
  uploadController.uploadFiles
);

router.delete(
  "/",
  authenticate,
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.CUSTOMER),
  validate(deleteUploadSchema),
  uploadController.deleteFiles
);

export default router;
