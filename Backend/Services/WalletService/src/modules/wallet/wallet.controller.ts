import { Request, Response } from "express";
import { WalletOwnerType } from "@prisma/client";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { UserServiceClient } from "../../integrations/user.service";
import {
  ListTransactionsQueryDto,
  OrderTransactionParamDto,
  NegativeWalletListResponseDto,
  OwnerParamDto,
  ReferenceTransactionParamDto,
  TopupBodyDto,
  TopupParamDto,
  TopupQueryDto,
  TopupListResponseDto,
  WalletTopupResponseDto,
} from "./wallet.dto";
import { walletService } from "./wallet.bootstrap";

export class WalletController {
  private readonly userServiceClient = new UserServiceClient();

  private getAuthContext(req: Request) {
    if (!req.auth?.userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return req.auth;
  }

  private resolveOwnerTypeFromAuth(req: Request): WalletOwnerType {
    const auth = this.getAuthContext(req);

    if (auth.roles.includes("ADMIN")) {
      return "ADMIN";
    }

    if (auth.roles.includes("MERCHANT")) {
      return "MERCHANT";
    }

    if (auth.roles.includes("SHIPPER")) {
      return "SHIPPER";
    }

    throw new ApiError(HTTP_STATUS.FORBIDDEN, "Wallet access is not allowed for this role");
  }

  private async resolveOwnerId(req: Request, ownerType: WalletOwnerType) {
    const auth = this.getAuthContext(req);

    if (ownerType === "ADMIN") {
      return auth.userId;
    }

    if (!auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid access token");
    }

    if (ownerType === "MERCHANT") {
      const merchant = await this.userServiceClient.getMerchantByUserId(auth.userId, auth.token);
      return merchant?.id ?? auth.merchantId ?? auth.userId;
    }

    const shipper = await this.userServiceClient.getShipperByUserId(auth.userId, auth.token);
    return shipper?.id ?? auth.shipperId ?? auth.userId;
  }

  private getRequestIp(req: Request) {
    const forwardedFor = req.headers["x-forwarded-for"];
    if (typeof forwardedFor === "string" && forwardedFor.trim().length > 0) {
      return forwardedFor.split(",")[0].trim();
    }

    return req.ip || "127.0.0.1";
  }

  getMyWallet = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const wallet = await walletService.getMyWallet(ownerType, ownerId);
    return Send.success(res, wallet, "Wallet fetched successfully");
  });

  getMyTransactions = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const query = (req.validated?.query ?? {}) as ListTransactionsQueryDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const result = await walletService.getMyTransactions(ownerType, ownerId, query);
    return Send.success(res, result, "Wallet transactions fetched successfully");
  });

  getMyTransactionsByReference = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const query = (req.validated?.query ?? {}) as ListTransactionsQueryDto;
    const params = (req.validated?.params ?? {}) as ReferenceTransactionParamDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const result = await walletService.getMyTransactionsByReference(ownerType, ownerId, params, query);
    return Send.success(res, result, "Wallet transactions fetched successfully");
  });

  getMyTransactionsByOrderId = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const query = (req.validated?.query ?? {}) as ListTransactionsQueryDto;
    const params = (req.validated?.params ?? {}) as OrderTransactionParamDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const result = await walletService.getMyTransactionsByOrderId(ownerType, ownerId, params, query);
    return Send.success(res, result, "Wallet transactions fetched successfully");
  });

  createTopupVnpayPaymentUrl = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const body = (req.validated?.body ?? {}) as TopupBodyDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);

    const paymentUrl = await walletService.createTopupPaymentUrl(
      ownerType,
      ownerId,
      this.getRequestIp(req),
      body,
    );

    return Send.success(res, paymentUrl, "Wallet VNPay topup URL created successfully");
  });

  getMyTopups = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const query = (req.validated?.query ?? {}) as TopupQueryDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const result: TopupListResponseDto = await walletService.getMyTopups(ownerType, ownerId, query);
    return Send.success(res, result, "Wallet topups fetched successfully");
  });

  getMyTopupById = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const params = (req.validated?.params ?? {}) as TopupParamDto;
    const ownerId = await this.resolveOwnerId(req, ownerType);
    const topup: WalletTopupResponseDto = await walletService.getMyTopupById(ownerType, ownerId, params.topupId);
    return Send.success(res, topup, "Wallet topup fetched successfully");
  });

  handleTopupVnpayReturn = asyncHandler(async (req: Request, res: Response) => {
    const result = await walletService.handleTopupVnpayReturn(
      req.query as Record<string, string | string[] | undefined>,
    );
    return Send.success(res, result, result.message);
  });

  handleTopupVnpayIpn = asyncHandler(async (req: Request, res: Response) => {
    const result = await walletService.handleTopupVnpayIpn(
      req.query as Record<string, string | string[] | undefined>,
    );
    return Send.success(res, result, result.message);
  });

  getWalletByOwner = asyncHandler(async (req: Request, res: Response) => {
    const { ownerType, ownerId } = req.validated?.params as OwnerParamDto;
    const wallet = await walletService.getMyWallet(ownerType, ownerId);
    return Send.success(res, wallet, "Wallet fetched successfully");
  });

  getNegativeWallets = asyncHandler(async (req: Request, res: Response) => {
    const query = (req.validated?.query ?? {}) as ListTransactionsQueryDto;
    const result: NegativeWalletListResponseDto = await walletService.getNegativeWallets(query);
    return Send.success(res, result, "Negative wallets fetched successfully");
  });
}
