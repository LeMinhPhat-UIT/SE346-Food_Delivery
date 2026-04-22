import { NextFunction, Request, Response } from "express";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";
import Send from "../utils/response";
import { logger } from "../utils/logger";

export const errorMiddleware = (
  error: unknown,
  _req: Request,
  res: Response,
  _next: NextFunction
) => {
  if (error instanceof ApiError) {
    return res.status(error.statusCode).json({
      ok: false,
      message: error.message,
      data: error.details ?? null,
    });
  }

  logger.error("Unhandled error", error);

  return Send.error(
    res,
    null,
    "Internal server error",
    HTTP_STATUS.INTERNAL_SERVER_ERROR
  );
};
