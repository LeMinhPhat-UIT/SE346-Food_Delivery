import { NextFunction, Request, Response } from "express";
import { ZodTypeAny } from "zod";
import { ApiError } from "../utils/apiError";
import { HTTP_STATUS } from "../constants/httpStatus";

export const validate = (schema: ZodTypeAny, source: "body" | "query" | "params" = "body") => {
  return (req: Request, _res: Response, next: NextFunction) => {
    const result = schema.safeParse(req[source]);

    if (!result.success) {
      const message = result.error.issues.map((issue) => issue.message).join(", ");
      return next(new ApiError(HTTP_STATUS.BAD_REQUEST, message));
    }

    (req as unknown as Record<string, unknown>)[source] = result.data;
    next();
  };
};
