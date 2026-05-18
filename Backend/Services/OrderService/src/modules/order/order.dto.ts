import { z } from "zod";
import { checkoutPreviewBodySchema } from "./order.schema";
import { PaymentMethod } from "@prisma/client";

export type CheckoutPreviewDto = z.infer<typeof checkoutPreviewBodySchema>;

export type CheckoutPreviewItemDto = {
  id: string;
  productId: string;
  productName: string;
  productImage: string | null;
  note: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  selectedOptions: Array<{
    optionId: string;
    name: string;
    values: Array<{
      valueId: string;
      name: string;
      additionalPrice: number;
    }>;
  }>;
};

export type CheckoutAddressDto = {
  id: string;
  addressLine: string;
  ward: string | null;
  district: string | null;
  city: string | null;
  lat: number;
  lng: number;
  recipientName: string | null;
  phone: string | null;
};

export type CheckoutMerchantAddressDto = {
  id: string;
  addressLine: string;
  ward: string | null;
  district: string | null;
  city: string | null;
  lat: number;
  lng: number;
};

export type AppliedVoucherPreviewDto = {
  id: string;
  code: string;
  name: string;
  discountTarget: "SUBTOTAL" | "DELIVERY_FEE";
  discountAmount: number;
};

export type CheckoutPreviewResponseDto = {
  userId: string;
  merchantId: string;
  paymentMethod: PaymentMethod | null;
  items: CheckoutPreviewItemDto[];
  userAddress: CheckoutAddressDto;
  merchantAddress: CheckoutMerchantAddressDto;
  subtotal: number;
  deliveryFee: number;
  voucherDiscount: number;
  total: number;
  distanceKm: number;
  appliedVoucher: AppliedVoucherPreviewDto | null;
  itemCount: number;
};
