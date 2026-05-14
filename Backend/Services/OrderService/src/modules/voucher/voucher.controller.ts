import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import {
  CreateVoucherDto,
  UpdateVoucherDto,
  UpdateVoucherStatusDto,
  ValidateVoucherDto,
  VoucherQueryDto,
} from "./voucher.dto";
import { VoucherRepository } from "./voucher.repository";
import { VoucherService } from "./voucher.service";

const voucherRepository = new VoucherRepository();
const voucherService = new VoucherService(voucherRepository);

export class VoucherController {
  getAllVouchers = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as VoucherQueryDto;
    const vouchers = await voucherService.getAllVouchers(filters);

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
    const voucher = await voucherService.createVoucher(payload);

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
    const voucher = await voucherService.updateVoucher(id, payload);

    return Send.success(res, voucher, "Voucher updated successfully");
  });

  updateVoucherStatus = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateVoucherStatusDto;
    const voucher = await voucherService.updateVoucherStatus(id, payload);

    return Send.success(res, voucher, "Voucher status updated successfully");
  });

  restoreVoucher = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const voucher = await voucherService.restoreVoucher(id);

    return Send.success(res, voucher, "Voucher restored successfully");
  });

  deleteVoucher = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const voucher = await voucherService.deleteVoucher(id);

    return Send.success(res, voucher, "Voucher deleted successfully");
  });

  validateVoucher = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as ValidateVoucherDto;
    const result = await voucherService.validateVoucher(payload);

    return Send.success(res, result, "Voucher validated successfully");
  });
}
