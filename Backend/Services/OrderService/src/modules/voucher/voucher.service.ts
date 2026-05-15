import { HTTP_STATUS } from "../../constants/httpStatus";
import { ROLES } from "../../constants/roles";
import { ApiError } from "../../utils/apiError";
import {
  CreateVoucherDto,
  UpdateVoucherDto,
  UpdateVoucherStatusDto,
  ValidateVoucherDto,
  VoucherActorContext,
  VoucherListResponseDto,
  VoucherQueryDto,
  VoucherResponseDto,
  VoucherValidationResponseDto,
} from "./voucher.dto";
import { toVoucherResponseDto } from "./voucher.mapper";
import { VoucherRecord, VoucherRepository } from "./voucher.repository";

export class VoucherService {
  constructor(private readonly voucherRepository: VoucherRepository) {}

  async getAllVouchers(
    filters: VoucherQueryDto,
    actor?: VoucherActorContext,
  ): Promise<VoucherListResponseDto> {
    const effectiveFilters = { ...filters };

    if (this.isMerchant(actor) && actor?.merchantId) {
      effectiveFilters.merchantId = actor.merchantId;
    }

    const { items, totalCount } = await this.voucherRepository.findAll(effectiveFilters);

    return {
      items: items.map(toVoucherResponseDto),
      totalCount,
      page: effectiveFilters.page,
      limit: effectiveFilters.limit,
      totalPages: Math.ceil(totalCount / effectiveFilters.limit),
    };
  }

  async getVoucherById(id: string): Promise<VoucherResponseDto> {
    const voucher = await this.ensureVoucherExists(id);
    return toVoucherResponseDto(voucher);
  }

  async getVoucherByCode(code: string): Promise<VoucherResponseDto> {
    const voucher = await this.voucherRepository.findByCode(this.normalizeCode(code));

    if (!voucher) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Voucher not found");
    }

    return toVoucherResponseDto(voucher);
  }

  async createVoucher(
    data: CreateVoucherDto,
    actor: VoucherActorContext,
  ): Promise<VoucherResponseDto> {
    await this.ensureVoucherCodeAvailable(this.normalizeCode(data.code));

    const normalizedPayload = this.normalizeVoucherPayload(data, actor);

    const voucher = await this.voucherRepository.create({
      ...normalizedPayload,
      code: this.normalizeCode(normalizedPayload.code),
    });

    return toVoucherResponseDto(voucher);
  }

  async updateVoucher(
    id: string,
    data: UpdateVoucherDto,
    actor: VoucherActorContext,
  ): Promise<VoucherResponseDto> {
    const existingVoucher = await this.ensureVoucherExists(id);
    this.assertVoucherOwnership(existingVoucher, actor);

    if (data.code) {
      await this.ensureVoucherCodeAvailable(this.normalizeCode(data.code), id);
    }

    const normalizedPayload = this.normalizeVoucherPayload(data, actor);
    const voucher = await this.voucherRepository.update(id, {
      ...normalizedPayload,
      ...(normalizedPayload.code
        ? { code: this.normalizeCode(normalizedPayload.code) }
        : {}),
    });

    return toVoucherResponseDto(voucher);
  }

  async updateVoucherStatus(
    id: string,
    data: UpdateVoucherStatusDto,
    actor: VoucherActorContext,
  ): Promise<VoucherResponseDto> {
    const existingVoucher = await this.ensureVoucherExists(id);
    this.assertVoucherOwnership(existingVoucher, actor);

    const voucher = await this.voucherRepository.update(id, {
      isActive: data.isActive,
    });

    return toVoucherResponseDto(voucher);
  }

  async restoreVoucher(
    id: string,
    actor: VoucherActorContext,
  ): Promise<VoucherResponseDto> {
    const voucher = await this.voucherRepository.findByIdIncludingDeleted(id);

    if (!voucher) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Voucher not found");
    }

    this.assertVoucherOwnership(voucher, actor);

    if (voucher.deletedAt === null) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher is already active");
    }

    const restoredVoucher = await this.voucherRepository.update(id, {
      deletedAt: null,
      isActive: true,
    });

    return toVoucherResponseDto(restoredVoucher);
  }

  async deleteVoucher(
    id: string,
    actor: VoucherActorContext,
  ): Promise<VoucherResponseDto> {
    const existingVoucher = await this.ensureVoucherExists(id);
    this.assertVoucherOwnership(existingVoucher, actor);

    const voucher = await this.voucherRepository.update(id, {
      deletedAt: new Date(),
      isActive: false,
    });

    return toVoucherResponseDto(voucher);
  }

  async validateVoucher(
    payload: ValidateVoucherDto,
  ): Promise<VoucherValidationResponseDto> {
    const code = this.normalizeCode(payload.code);
    const voucher = await this.voucherRepository.findByCode(code);

    if (!voucher) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Voucher not found");
    }

    this.assertVoucherUsable(voucher, payload);

    const [totalUsageCount, userUsageCount] = await Promise.all([
      this.voucherRepository.countTotalUsage(voucher.id),
      this.voucherRepository.countUserUsage(voucher.id, payload.userId),
    ]);

    if (voucher.usageLimit !== null && totalUsageCount >= voucher.usageLimit) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Voucher usage limit has been reached",
      );
    }

    if (userUsageCount >= voucher.perUserLimit) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "User has reached the voucher usage limit",
      );
    }

    const appliedAmount =
      voucher.discountTarget === "DELIVERY_FEE"
        ? payload.deliveryFee
        : payload.subtotal;

    const discountAmount = this.calculateDiscountAmount(voucher, appliedAmount);
    const finalSubtotal =
      voucher.discountTarget === "SUBTOTAL"
        ? Math.max(payload.subtotal - discountAmount, 0)
        : payload.subtotal;
    const finalDeliveryFee =
      voucher.discountTarget === "DELIVERY_FEE"
        ? Math.max(payload.deliveryFee - discountAmount, 0)
        : payload.deliveryFee;

    return {
      voucher: toVoucherResponseDto(voucher),
      discountAmount,
      discountTarget: voucher.discountTarget,
      appliedAmount,
      finalSubtotal,
      finalDeliveryFee,
      finalTotal: finalSubtotal + finalDeliveryFee,
    };
  }

  private async ensureVoucherExists(id: string) {
    const voucher = await this.voucherRepository.findById(id);

    if (!voucher) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Voucher not found");
    }

    return voucher;
  }

  private async ensureVoucherCodeAvailable(code: string, voucherId?: string) {
    const existingVoucher =
      await this.voucherRepository.findByCodeIncludingDeleted(code);

    if (existingVoucher && existingVoucher.id !== voucherId) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher code already exists");
    }
  }

  private normalizeCode(code: string) {
    return code.trim().toUpperCase();
  }

  private normalizeVoucherPayload<T extends CreateVoucherDto | UpdateVoucherDto>(
    data: T,
    actor: VoucherActorContext,
  ): T {
    if (this.isMerchant(actor)) {
      return {
        ...data,
        merchantId: actor.merchantId ?? null,
      };
    }

    return data;
  }

  private assertVoucherOwnership(
    voucher: VoucherRecord,
    actor: VoucherActorContext,
  ) {
    if (!this.isMerchant(actor)) {
      return;
    }

    if (!actor.merchantId) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant context is missing");
    }

    if (voucher.merchantId !== actor.merchantId) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "You can only modify vouchers that belong to your own store",
      );
    }
  }

  private isMerchant(actor?: VoucherActorContext) {
    return Boolean(
      actor?.roles.some(
        (role) => role.toLowerCase() === ROLES.MERCHANT.toLowerCase(),
      ),
    );
  }

  private assertVoucherUsable(
    voucher: VoucherRecord,
    payload: ValidateVoucherDto,
  ) {
    const now = new Date();

    if (voucher.deletedAt) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher has been deleted");
    }

    if (!voucher.isActive) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher is inactive");
    }

    if (voucher.startDate > now) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher is not active yet");
    }

    if (voucher.endDate < now) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Voucher has expired");
    }

    if (
      voucher.merchantId !== null &&
      voucher.merchantId !== payload.merchantId
    ) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Voucher is not applicable to this merchant",
      );
    }

    if (
      voucher.minOrderAmount !== null &&
      payload.subtotal < Number(voucher.minOrderAmount)
    ) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        `Order subtotal must be at least ${Number(voucher.minOrderAmount)}`,
      );
    }
  }

  private calculateDiscountAmount(voucher: VoucherRecord, appliedAmount: number) {
    if (appliedAmount <= 0) {
      return 0;
    }

    const discountValue = Number(voucher.discountValue);

    if (voucher.discountType === "FIXED") {
      return Math.min(discountValue, appliedAmount);
    }

    const rawDiscount = (appliedAmount * discountValue) / 100;
    const cappedDiscount =
      voucher.maxDiscount === null
        ? rawDiscount
        : Math.min(rawDiscount, Number(voucher.maxDiscount));

    return Math.min(cappedDiscount, appliedAmount);
  }
}
