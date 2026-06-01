import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { UserServiceClient } from "../../integrations/user.service";
import { DateRangeQueryDto } from "./report.dto";
import { reportService } from "./report.bootstrap";

export class ReportController {
  private readonly userServiceClient = new UserServiceClient();

  private getAuthContext(req: Request) {
    if (!req.auth?.userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return req.auth;
  }

  private async resolveMerchantProfileId(req: Request) {
    const auth = this.getAuthContext(req);

    if (auth.merchantId && auth.merchantId !== auth.userId) {
      return auth.merchantId;
    }

    if (!auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid access token");
    }

    const merchant = await this.userServiceClient.getMerchantByUserId(auth.userId, auth.token);
    return merchant?.id ?? auth.merchantId ?? auth.userId;
  }

  private async resolveShipperProfileId(req: Request) {
    const auth = this.getAuthContext(req);

    if (auth.shipperId && auth.shipperId !== auth.userId) {
      return auth.shipperId;
    }

    if (!auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid access token");
    }

    const shipper = await this.userServiceClient.getShipperByUserId(auth.userId, auth.token);
    return shipper?.id ?? auth.shipperId ?? auth.userId;
  }

  getAdminOverview = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const result = await reportService.getAdminOverview(query);
    return Send.success(res, result, "Admin overview fetched successfully");
  });

  getMerchantOverview = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const merchantId = await this.resolveMerchantProfileId(req);
    const result = await reportService.getMerchantOverview(merchantId, query);
    return Send.success(res, result, "Merchant overview fetched successfully");
  });

  getShipperOverview = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const shipperId = await this.resolveShipperProfileId(req);
    const result = await reportService.getShipperOverview(shipperId, query);
    return Send.success(res, result, "Shipper overview fetched successfully");
  });

  getTopMerchants = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const result = await reportService.getTopMerchants(query);
    return Send.success(res, result, "Top merchants fetched successfully");
  });

  getTopShippers = asyncHandler(async (req: Request, res: Response) => {
    this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const result = await reportService.getTopShippers(query);
    return Send.success(res, result, "Top shippers fetched successfully");
  });

  getTopProducts = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as DateRangeQueryDto;
    const merchantId = auth.roles.includes("MERCHANT") ? await this.resolveMerchantProfileId(req) : undefined;
    const result = await reportService.getTopProducts(query, merchantId);
    return Send.success(res, result, "Top products fetched successfully");
  });
}
