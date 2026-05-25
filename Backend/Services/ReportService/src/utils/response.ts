import { Response } from "express";

const Send = {
  success<T>(res: Response, data: T, message = "Success", status = 200) {
    return res.status(status).json({
      ok: true,
      message,
      data,
    });
  },
};

export default Send;
