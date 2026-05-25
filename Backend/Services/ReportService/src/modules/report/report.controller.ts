import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { DateRangeQueryDto } from "./report.dto";
import { reportService } from "./report.bootstrap";

export class ReportController {
  private getAuthContext(req: Request) {
    if (!req.auth?.userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return req.auth;
  }

  getAdminOverview = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const result = await reportService.getAdminOverview(query);
    return Send.success(res, result, "Admin overview fetched successfully");
  });

  getMerchantOverview = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const merchantId = auth.merchantId ?? auth.userId;
    const result = await reportService.getMerchantOverview(merchantId, query);
    return Send.success(res, result, "Merchant overview fetched successfully");
  });

  getShipperOverview = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const shipperId = auth.shipperId ?? auth.userId;
    const result = await reportService.getShipperOverview(shipperId, query);
    return Send.success(res, result, "Shipper overview fetched successfully");
  });
}
