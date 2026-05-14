import { z } from "zod";
import {
  createVoucherBodySchema,
  updateVoucherBodySchema,
  updateVoucherStatusBodySchema,
  validateVoucherBodySchema,
  voucherQuerySchema,
} from "./voucher.schema";

export type CreateVoucherDto = z.infer<typeof createVoucherBodySchema>;
export type UpdateVoucherDto = z.infer<typeof updateVoucherBodySchema>;
export type UpdateVoucherStatusDto = z.infer<
  typeof updateVoucherStatusBodySchema
>;
export type VoucherQueryDto = z.infer<typeof voucherQuerySchema>;
export type ValidateVoucherDto = z.infer<typeof validateVoucherBodySchema>;

export type VoucherAvailabilityStatus =
  | "active"
  | "upcoming"
  | "expired"
  | "inactive"
  | "deleted";

export type VoucherResponseDto = {
  id: string;
  code: string;
  name: string;
  description: string | null;
  discountType: "PERCENTAGE" | "FIXED";
  discountValue: number;
  maxDiscount: number | null;
  minOrderAmount: number | null;
  discountTarget: "SUBTOTAL" | "DELIVERY_FEE";
  merchantId: string | null;
  usageLimit: number | null;
  perUserLimit: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdAt: string;
  deletedAt: string | null;
  usedCount: number;
  remainingUsage: number | null;
  availability: VoucherAvailabilityStatus;
};

export type VoucherListResponseDto = {
  items: VoucherResponseDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
};

export type VoucherValidationResponseDto = {
  voucher: VoucherResponseDto;
  discountAmount: number;
  discountTarget: "SUBTOTAL" | "DELIVERY_FEE";
  appliedAmount: number;
  finalSubtotal: number;
  finalDeliveryFee: number;
  finalTotal: number;
};
