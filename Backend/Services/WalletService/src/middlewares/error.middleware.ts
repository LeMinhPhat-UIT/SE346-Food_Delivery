import { NextFunction, Request, Response } from "express";
import { ApiError } from "../utils/apiError";
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
      details: error.details,
    });
  }

  logger.error("Unhandled error in Wallet Service", error);

  return res.status(500).json({
    ok: false,
    message: "Internal server error",
  });
};
