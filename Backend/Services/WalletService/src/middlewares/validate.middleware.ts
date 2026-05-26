import { NextFunction, Request, Response } from "express";
import { ZodTypeAny } from "zod";
import { ApiError } from "../utils/apiError";
import { HTTP_STATUS } from "../constants/httpStatus";

type SchemaBag = {
  body?: ZodTypeAny;
  params?: ZodTypeAny;
  query?: ZodTypeAny;
};

export const validate =
  (schemas: SchemaBag) =>
  (req: Request, _res: Response, next: NextFunction) => {
    try {
      req.validated = {};

      if (schemas.body) {
        req.validated.body = schemas.body.parse(req.body);
      }

      if (schemas.params) {
        req.validated.params = schemas.params.parse(req.params);
      }

      if (schemas.query) {
        req.validated.query = schemas.query.parse(req.query);
      }

      next();
    } catch (error) {
      next(new ApiError(HTTP_STATUS.BAD_REQUEST, "Validation failed", error));
    }
  };
