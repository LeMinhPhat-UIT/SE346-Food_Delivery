import { NextFunction, Request, Response } from "express";
import { ApiError } from "../utils/apiError";
import { HTTP_STATUS } from "../constants/httpStatus";
import { logger } from "../utils/logger";

export const errorMiddleware = (
  error: unknown,
  _req: Request,
  res: Response,
  _next: NextFunction,
) => {
  if (error instanceof ApiError) {
    return res.status(error.statusCode).json({
      ok: false,
      message: error.message,
    });
  }

  logger.error("Unhandled chat service error", error);

  return res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
    ok: false,
    message: "Internal server error",
  });
};
