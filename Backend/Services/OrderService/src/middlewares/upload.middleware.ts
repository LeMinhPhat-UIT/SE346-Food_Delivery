import multer from "multer";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

const allowedMimeTypes = new Set([
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/jpg",
]);

export const uploadMiddleware = multer({
  storage: multer.memoryStorage(),
  limits: {
    fileSize: 5 * 1024 * 1024,
    files: 10,
  },
  fileFilter: (_req, file, cb) => {
    if (!allowedMimeTypes.has(file.mimetype)) {
      return cb(
        new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Only JPG, JPEG, PNG, and WEBP images are allowed"
        )
      );
    }

    cb(null, true);
  },
});
