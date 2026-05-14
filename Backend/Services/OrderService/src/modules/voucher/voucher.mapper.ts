import {
  VoucherAvailabilityStatus,
  VoucherResponseDto,
} from "./voucher.dto";
import { VoucherRecord } from "./voucher.repository";

export const getVoucherAvailabilityStatus = (
  voucher: Pick<VoucherRecord, "deletedAt" | "isActive" | "startDate" | "endDate">,
): VoucherAvailabilityStatus => {
  const now = new Date();

  if (voucher.deletedAt) {
    return "deleted";
  }

  if (!voucher.isActive) {
    return "inactive";
  }

  if (voucher.startDate > now) {
    return "upcoming";
  }

  if (voucher.endDate < now) {
    return "expired";
  }

  return "active";
};

export const toVoucherResponseDto = (
  voucher: VoucherRecord,
): VoucherResponseDto => {
  return {
    id: voucher.id,
    code: voucher.code,
    name: voucher.name,
    description: voucher.description,
    discountType: voucher.discountType,
    discountValue: Number(voucher.discountValue),
    maxDiscount: voucher.maxDiscount === null ? null : Number(voucher.maxDiscount),
    minOrderAmount:
      voucher.minOrderAmount === null ? null : Number(voucher.minOrderAmount),
    discountTarget: voucher.discountTarget,
    merchantId: voucher.merchantId,
    usageLimit: voucher.usageLimit,
    perUserLimit: voucher.perUserLimit,
    startDate: voucher.startDate.toISOString(),
    endDate: voucher.endDate.toISOString(),
    isActive: voucher.isActive,
    createdAt: voucher.createdAt.toISOString(),
    deletedAt: voucher.deletedAt ? voucher.deletedAt.toISOString() : null,
    usedCount: voucher._count.usages,
    remainingUsage:
      voucher.usageLimit === null
        ? null
        : Math.max(voucher.usageLimit - voucher._count.usages, 0),
    availability: getVoucherAvailabilityStatus(voucher),
  };
};
