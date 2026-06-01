import { NextFunction, Request, Response } from "express";
import { ApiError } from "../utils/apiError";
import { HTTP_STATUS } from "../constants/httpStatus";
import { env } from "../config/env.config";
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

  const normalizedError =
    error instanceof Error
      ? {
          name: error.name,
          message: error.message,
          stack: error.stack,
        }
      : error;

  logger.error("Unhandled chat service error", normalizedError);

  const debugMessage =
    env.NODE_ENV === "development" && error instanceof Error
      ? error.message
      : "Internal server error";

  return res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
    ok: false,
    message: debugMessage,
  });
};
