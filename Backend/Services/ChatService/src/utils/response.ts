import { Response } from "express";

type SuccessResponseOptions<T> = {
  message: string;
  data?: T;
  statusCode?: number;
};

export const sendSuccess = <T>(res: Response, options: SuccessResponseOptions<T>) => {
  const { message, data, statusCode = 200 } = options;

  return res.status(statusCode).json({
    ok: true,
    message,
    data,
  });
};
