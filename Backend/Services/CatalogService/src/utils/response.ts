import { Response } from "express";
import { HTTP_STATUS } from "../constants/httpStatus";

export default class Send {
  static success(
    res: Response,
    data: unknown,
    message = "success",
    statusCode: number = HTTP_STATUS.OK
  ) {
    return res.status(statusCode).json({
      ok: true,
      message,
      data,
    });
  }

  static error(
    res: Response,
    data: unknown,
    message = "error",
    statusCode = HTTP_STATUS.INTERNAL_SERVER_ERROR
  ) {
    return res.status(statusCode).json({
      ok: false,
      message,
      data,
    });
  }
}
