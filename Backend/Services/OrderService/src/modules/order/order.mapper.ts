import {
  MerchantAddress,
  UserAddress,
} from "../../integrations/user.service";
import { CartResponseDto } from "../cart/cart.dto";
import { VoucherValidationResponseDto } from "../voucher/voucher.dto";
import { CheckoutPreviewResponseDto } from "./order.dto";

type BuildCheckoutPreviewInput = {
  userId: string;
  cart: CartResponseDto;
  paymentMethod?: "COD" | "VNPAY";
  userAddress: UserAddress;
  merchantAddress: MerchantAddress;
  deliveryFee: number;
  distanceKm: number;
  voucherResult: VoucherValidationResponseDto | null;
};

export const toCheckoutPreviewResponseDto = ({
  userId,
  cart,
  paymentMethod,
  userAddress,
  merchantAddress,
  deliveryFee,
  distanceKm,
  voucherResult,
}: BuildCheckoutPreviewInput): CheckoutPreviewResponseDto => {
  const subtotal = cart.subtotal;
  const voucherDiscount = voucherResult?.discountAmount ?? 0;
  const total = voucherResult
    ? voucherResult.finalTotal
    : subtotal + deliveryFee;

  return {
    userId,
    merchantId: cart.merchantId,
    paymentMethod: paymentMethod ?? null,
    items: cart.items.map((item) => ({
      id: item.id,
      productId: item.productId,
      productName: item.productName,
      productImage: item.productImage,
      note: item.note,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      lineTotal: item.lineTotal,
      selectedOptions: item.selectedOptions,
    })),
    userAddress: {
      id: userAddress.id,
      addressLine: userAddress.addressLine,
      ward: userAddress.ward ?? null,
      district: userAddress.district ?? null,
      city: userAddress.city ?? null,
      lat: Number(userAddress.lat),
      lng: Number(userAddress.lng),
      recipientName: userAddress.recipientName ?? null,
      phone: userAddress.phone ?? null,
    },
    merchantAddress: {
      id: merchantAddress.id,
      addressLine: merchantAddress.addressLine,
      ward: merchantAddress.ward ?? null,
      district: merchantAddress.district ?? null,
      city: merchantAddress.city ?? null,
      lat: Number(merchantAddress.lat),
      lng: Number(merchantAddress.lng),
    },
    subtotal,
    deliveryFee: voucherResult ? voucherResult.finalDeliveryFee : deliveryFee,
    voucherDiscount,
    total,
    distanceKm,
    appliedVoucher: voucherResult
      ? {
          id: voucherResult.voucher.id,
          code: voucherResult.voucher.code,
          name: voucherResult.voucher.name,
          discountTarget: voucherResult.discountTarget,
          discountAmount: voucherResult.discountAmount,
        }
      : null,
    itemCount: cart.totalQuantity,
  };
};
