import {
  MerchantAddress,
  UserAddress,
} from "../../integrations/user.service";
import { CartResponseDto } from "../cart/cart.dto";
import { VoucherValidationResponseDto } from "../voucher/voucher.dto";
import {
  CheckoutPreviewResponseDto,
  CreateOrderResponseDto,
  MyOrdersResponseDto,
  OrderDetailResponseDto,
} from "./order.dto";

type BuildCheckoutPreviewInput = {
  userId: string;
  cart: CartResponseDto;
  paymentMethod?: "COD" | "VNPAY";
  userAddress: UserAddress;
  merchantAddress: MerchantAddress;
  deliveryFee: number;
  distanceKm: number;
  estimatedTimeMinutes: number | null;
  deliveryFeeCurrency: string;
  isWithinDeliveryRadius: boolean | null;
  maxDeliveryDistanceKm: number | null;
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
  estimatedTimeMinutes,
  deliveryFeeCurrency,
  isWithinDeliveryRadius,
  maxDeliveryDistanceKm,
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
    estimatedTimeMinutes,
    deliveryFeeCurrency,
    isWithinDeliveryRadius,
    maxDeliveryDistanceKm,
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

export const toCreateOrderResponseDto = (order: {
  id: string;
  orderNumber: string;
  userId: string;
  merchantId: string;
  paymentMethod: string;
  paymentStatus: string;
  status: string;
  subtotal: number;
  deliveryFee: number;
  discountAmount: number;
  totalAmount: number;
  voucherId: string | null;
  createdAt: Date;
  items: CheckoutPreviewResponseDto["items"];
}): CreateOrderResponseDto => {
  return {
    id: order.id,
    orderNumber: order.orderNumber,
    userId: order.userId,
    merchantId: order.merchantId,
    paymentMethod: order.paymentMethod as CreateOrderResponseDto["paymentMethod"],
    paymentStatus: order.paymentStatus,
    status: order.status,
    subtotal: order.subtotal,
    deliveryFee: order.deliveryFee,
    discountAmount: order.discountAmount,
    totalAmount: order.totalAmount,
    voucherId: order.voucherId,
    createdAt: order.createdAt.toISOString(),
    items: order.items,
  };
};

type OrderListRecord = {
  id: string;
  orderNumber: string;
  merchantId: string;
  shipperId: string | null;
  merchantName: string;
  merchantAvatar: string | null;
  subtotal: { toString(): string } | number;
  deliveryFee: { toString(): string } | number;
  discountAmount: { toString(): string } | number;
  totalAmount: { toString(): string } | number;
  paymentMethod: CreateOrderResponseDto["paymentMethod"];
  paymentStatus: string;
  status: string;
  createdAt: Date;
  items: Array<{
    id: string;
    productId: string;
    productName: string;
    productImage: string | null;
    quantity: number;
  }>;
};

export const toMyOrdersResponseDto = (
  orders: OrderListRecord[],
  meta: { totalCount: number; page: number; limit: number },
): MyOrdersResponseDto => {
  return {
    items: orders.map((order) => ({
      id: order.id,
      orderNumber: order.orderNumber,
      merchantId: order.merchantId,
      shipperId: order.shipperId,
      merchantName: order.merchantName,
      merchantAvatar: order.merchantAvatar,
      subtotal: Number(order.subtotal),
      deliveryFee: Number(order.deliveryFee),
      discountAmount: Number(order.discountAmount),
      totalAmount: Number(order.totalAmount),
      paymentMethod: order.paymentMethod,
      paymentStatus: order.paymentStatus as MyOrdersResponseDto["items"][number]["paymentStatus"],
      status: order.status as MyOrdersResponseDto["items"][number]["status"],
      createdAt: order.createdAt.toISOString(),
      itemCount: order.items.reduce((sum, item) => sum + item.quantity, 0),
      previewItems: order.items.slice(0, 3).map((item) => ({
        id: item.id,
        productId: item.productId,
        productName: item.productName,
        productImage: item.productImage,
        quantity: item.quantity,
      })),
    })),
    totalCount: meta.totalCount,
    page: meta.page,
    limit: meta.limit,
    totalPages: Math.ceil(meta.totalCount / meta.limit),
  };
};

type OrderDetailRecord = {
  id: string;
  orderNumber: string;
  userId: string;
  merchantId: string;
  shipperId: string | null;
  merchantName: string;
  merchantAvatar: string | null;
  deliveryAddress: string;
  deliveryWard: string | null;
  deliveryDistrict: string | null;
  deliveryCity: string | null;
  deliveryLat: { toString(): string } | number | null;
  deliveryLng: { toString(): string } | number | null;
  recipientName: string;
  recipientPhone: string;
  subtotal: { toString(): string } | number;
  deliveryFee: { toString(): string } | number;
  discountAmount: { toString(): string } | number;
  totalAmount: { toString(): string } | number;
  paymentMethod: CreateOrderResponseDto["paymentMethod"];
  paymentStatus: string;
  status: string;
  cancelReason: string | null;
  cancelledBy: string | null;
  note: string | null;
  voucherId: string | null;
  createdAt: Date;
  updatedAt: Date;
  items: Array<{
    id: string;
    productId: string;
    productName: string;
    productImage: string | null;
    unitPrice: { toString(): string } | number;
    selectedOptions: unknown;
    quantity: number;
    note: string | null;
    createdAt: Date;
  }>;
  statusHistory: Array<{
    id: string;
    status: string;
    note: string | null;
    createdBy: string | null;
    createdAt: Date;
  }>;
};

export const toOrderDetailResponseDto = (
  order: OrderDetailRecord,
): OrderDetailResponseDto => {
  return {
    id: order.id,
    orderNumber: order.orderNumber,
    userId: order.userId,
    merchantId: order.merchantId,
    shipperId: order.shipperId,
    merchantName: order.merchantName,
    merchantAvatar: order.merchantAvatar,
    deliveryAddress: order.deliveryAddress,
    deliveryWard: order.deliveryWard,
    deliveryDistrict: order.deliveryDistrict,
    deliveryCity: order.deliveryCity,
    deliveryLat: order.deliveryLat !== null ? Number(order.deliveryLat) : null,
    deliveryLng: order.deliveryLng !== null ? Number(order.deliveryLng) : null,
    recipientName: order.recipientName,
    recipientPhone: order.recipientPhone,
    subtotal: Number(order.subtotal),
    deliveryFee: Number(order.deliveryFee),
    discountAmount: Number(order.discountAmount),
    totalAmount: Number(order.totalAmount),
    paymentMethod: order.paymentMethod,
    paymentStatus: order.paymentStatus as OrderDetailResponseDto["paymentStatus"],
    status: order.status as OrderDetailResponseDto["status"],
    cancelReason: order.cancelReason,
    cancelledBy: order.cancelledBy,
    note: order.note,
    voucherId: order.voucherId,
    createdAt: order.createdAt.toISOString(),
    updatedAt: order.updatedAt.toISOString(),
    items: order.items.map((item) => ({
      id: item.id,
      productId: item.productId,
      productName: item.productName,
      productImage: item.productImage,
      unitPrice: Number(item.unitPrice),
      selectedOptions: item.selectedOptions,
      quantity: item.quantity,
      note: item.note,
      createdAt: item.createdAt.toISOString(),
    })),
    statusHistory: order.statusHistory.map((history) => ({
      id: history.id,
      status: history.status as OrderDetailResponseDto["statusHistory"][number]["status"],
      note: history.note,
      createdBy: history.createdBy,
      createdAt: history.createdAt.toISOString(),
    })),
  };
};
