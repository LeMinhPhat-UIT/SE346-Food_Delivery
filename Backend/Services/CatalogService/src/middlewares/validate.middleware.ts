import { NextFunction, Request, Response } from "express";
import { z } from "zod";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

type ValidationSchema = {
  body?: z.ZodTypeAny;
  params?: z.ZodTypeAny;
  query?: z.ZodTypeAny;
};

export const validate = (schema: ValidationSchema) => {
  return (req: Request, _res: Response, next: NextFunction) => {
    try {
      const validated: Request["validated"] = {};

      if (schema.body) {
        validated.body = schema.body.parse(req.body);
      }

      if (schema.params) {
        validated.params = schema.params.parse(req.params);
      }

      if (schema.query) {
        validated.query = schema.query.parse(req.query);
      }

      req.validated = validated;

      next();
    } 
    catch (error) {
      if (error instanceof z.ZodError) {
        const message = error.issues.map((issue) => issue.message).join(", ");

        return next(
          new ApiError(HTTP_STATUS.BAD_REQUEST, message, error.flatten())
        );
      }

      return next(error);
    }
  };
};
