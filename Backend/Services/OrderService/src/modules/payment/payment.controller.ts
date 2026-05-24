import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import {
  CreateVnpayPaymentUrlDto,
  OrderIdParamDto,
} from "./payment.dto";
import { paymentService } from "./payment.bootstrap";

export class PaymentController {
  private getAuthContext(req: Request) {
    if (!req.auth?.userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return {
      userId: req.auth.userId,
    };
  }

  getPaymentByOrderId = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { orderId } = req.validated?.params as OrderIdParamDto;

    const payment = await paymentService.getPaymentByOrderId(auth.userId, orderId);
    return Send.success(res, payment, "Payment fetched successfully");
  });

  createVnpayPaymentUrl = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { orderId } = req.validated?.params as OrderIdParamDto;
    const payload = (req.validated?.body ?? {}) as CreateVnpayPaymentUrlDto;

    const paymentUrl = await paymentService.createVnpayPaymentUrl(
      auth.userId,
      orderId,
      req.ip || "127.0.0.1",
      payload,
    );

    return Send.success(res, paymentUrl, "VNPay payment URL created successfully");
  });

  handleVnpayReturn = asyncHandler(async (req: Request, res: Response) => {
    const result = await paymentService.handleVnpayReturn(req.query as Record<string, string | string[] | undefined>);
    return Send.success(res, result, result.message);
  });

  handleVnpayIpn = asyncHandler(async (req: Request, res: Response) => {
    const result = await paymentService.handleVnpayIpn(req.query as Record<string, string | string[] | undefined>);
    return Send.success(res, result, result.message);
  });
}

