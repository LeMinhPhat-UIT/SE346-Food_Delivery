import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { ListTransactionsQueryDto, OwnerParamDto, OwnerTypeParamDto } from "./wallet.dto";
import { walletService } from "./wallet.bootstrap";

export class WalletController {
  private getAuthContext(req: Request) {
    if (!req.auth?.userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return req.auth;
  }

  private resolveOwnerTypeFromAuth(req: Request): OwnerTypeParamDto["ownerType"] {
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

  getMyWallet = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const wallet = await walletService.getMyWallet(ownerType, auth.userId);
    return Send.success(res, wallet, "Wallet fetched successfully");
  });

  getMyTransactions = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const ownerType = this.resolveOwnerTypeFromAuth(req);
    const query = (req.validated?.query ?? {}) as ListTransactionsQueryDto;
    const result = await walletService.getMyTransactions(ownerType, auth.userId, query);
    return Send.success(res, result, "Wallet transactions fetched successfully");
  });

  getWalletByOwner = asyncHandler(async (req: Request, res: Response) => {
    const { ownerType, ownerId } = req.validated?.params as OwnerParamDto;
    const wallet = await walletService.getMyWallet(ownerType, ownerId);
    return Send.success(res, wallet, "Wallet fetched successfully");
  });
}
