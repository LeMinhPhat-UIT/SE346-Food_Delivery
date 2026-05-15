import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import {
  CreateVoucherDto,
  UpdateVoucherDto,
  UpdateVoucherStatusDto,
  ValidateVoucherDto,
  VoucherActorContext,
  VoucherQueryDto,
} from "./voucher.dto";
import { VoucherRepository } from "./voucher.repository";
import { VoucherService } from "./voucher.service";

const voucherRepository = new VoucherRepository();
const voucherService = new VoucherService(voucherRepository);

export class VoucherController {
  private getActorContext(req: Request): VoucherActorContext {
    const auth = req.auth;

    if (!auth) {
      throw new ApiError(401, "Invalid user context");
    }

    return {
      userId: auth.userId,
      roles: auth.roles,
      merchantId: auth.merchantId,
    };
  }

  getAllVouchers = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as VoucherQueryDto;
    const vouchers = await voucherService.getAllVouchers(
      filters,
      req.auth ? this.getActorContext(req) : undefined,
    );

    return Send.success(res, vouchers, "Vouchers fetched successfully");
  });

  getVoucherById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const voucher = await voucherService.getVoucherById(id);

    return Send.success(res, voucher, "Voucher fetched successfully");
  });

  getVoucherByCode = asyncHandler(async (req: Request, res: Response) => {
    const { code } = req.validated?.params as { code: string };
    const voucher = await voucherService.getVoucherByCode(code);

    return Send.success(res, voucher, "Voucher fetched successfully");
  });

  createVoucher = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as CreateVoucherDto;
    const voucher = await voucherService.createVoucher(
      payload,
      this.getActorContext(req),
    );

    return Send.success(
      res,
      voucher,
      "Voucher created successfully",
      HTTP_STATUS.CREATED,
    );
  });

  updateVoucher = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateVoucherDto;
    const voucher = await voucherService.updateVoucher(
      id,
      payload,
      this.getActorContext(req),
    );

    return Send.success(res, voucher, "Voucher updated successfully");
  });

  updateVoucherStatus = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateVoucherStatusDto;
    const voucher = await voucherService.updateVoucherStatus(
      id,
      payload,
      this.getActorContext(req),
    );

    return Send.success(res, voucher, "Voucher status updated successfully");
  });

  restoreVoucher = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const voucher = await voucherService.restoreVoucher(
      id,
      this.getActorContext(req),
    );

    return Send.success(res, voucher, "Voucher restored successfully");
  });

  deleteVoucher = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const voucher = await voucherService.deleteVoucher(
      id,
      this.getActorContext(req),
    );

    return Send.success(res, voucher, "Voucher deleted successfully");
  });

  validateVoucher = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as ValidateVoucherDto;
    const result = await voucherService.validateVoucher(payload);

    return Send.success(res, result, "Voucher validated successfully");
  });
}
