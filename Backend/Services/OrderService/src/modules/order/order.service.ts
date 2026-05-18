import { env } from "../../config/env.config";
import { HTTP_STATUS } from "../../constants/httpStatus";
import {
  MerchantAddress,
  UserAddress,
  UserServiceClient,
} from "../../integrations/user.service";
import { ApiError } from "../../utils/apiError";
import { CartService } from "../cart/cart.service";
import { VoucherRepository } from "../voucher/voucher.repository";
import { VoucherService } from "../voucher/voucher.service";
import { toCheckoutPreviewResponseDto } from "./order.mapper";
import { CheckoutPreviewDto, CheckoutPreviewResponseDto } from "./order.dto";
import { OrderRepository } from "./order.repository";

export class OrderService {
  private readonly voucherService = new VoucherService(new VoucherRepository());

  constructor(
    private readonly orderRepository: OrderRepository,
    private readonly cartService: CartService,
    private readonly userServiceClient: UserServiceClient,
  ) {}

  async previewCheckout(
    userId: string,
    token: string,
    payload: CheckoutPreviewDto,
  ): Promise<CheckoutPreviewResponseDto> {
    const cart = await this.cartService.getCartByMerchant(userId, payload.merchantId);

    if (cart.items.length === 0) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Cannot preview checkout because the merchant cart is empty",
      );
    }

    const [userAddress, merchantAddress] = await Promise.all([
      this.userServiceClient.getUserAddressById(userId, payload.addressId, token),
      this.userServiceClient.getMerchantPrimaryAddress(payload.merchantId),
    ]);

    this.assertAddressCoordinates(userAddress, merchantAddress);

    const distanceKm = this.calculateDistanceKm(
      Number(userAddress.lat),
      Number(userAddress.lng),
      Number(merchantAddress.lat),
      Number(merchantAddress.lng),
    );
    const deliveryFee = this.calculateDeliveryFee(distanceKm);

    const voucherResult = payload.voucherCode
      ? await this.voucherService.validateVoucher({
          code: payload.voucherCode,
          userId,
          merchantId: payload.merchantId,
          subtotal: cart.subtotal,
          deliveryFee,
        })
      : null;

    if (voucherResult) {
      await this.orderRepository.isVoucherUsedByOrder(voucherResult.voucher.id, userId);
    }

    return toCheckoutPreviewResponseDto({
      userId,
      cart,
      paymentMethod: payload.paymentMethod,
      userAddress,
      merchantAddress,
      deliveryFee,
      distanceKm,
      voucherResult,
    });
  }

  private assertAddressCoordinates(
    userAddress: UserAddress,
    merchantAddress: MerchantAddress,
  ) {
    const hasUserCoordinates =
      userAddress.lat !== null &&
      userAddress.lat !== undefined &&
      userAddress.lng !== null &&
      userAddress.lng !== undefined;
    const hasMerchantCoordinates =
      merchantAddress.lat !== null &&
      merchantAddress.lat !== undefined &&
      merchantAddress.lng !== null &&
      merchantAddress.lng !== undefined;

    if (!hasUserCoordinates) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Selected delivery address does not contain coordinates",
      );
    }

    if (!hasMerchantCoordinates) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Merchant address does not contain coordinates",
      );
    }
  }

  private calculateDistanceKm(
    startLat: number,
    startLng: number,
    endLat: number,
    endLng: number,
  ) {
    const earthRadiusKm = 6371;
    const toRadians = (value: number) => (value * Math.PI) / 180;
    const dLat = toRadians(endLat - startLat);
    const dLng = toRadians(endLng - startLng);
    const lat1 = toRadians(startLat);
    const lat2 = toRadians(endLat);

    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.sin(dLng / 2) * Math.sin(dLng / 2) *
        Math.cos(lat1) *
        Math.cos(lat2);

    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return Number((earthRadiusKm * c).toFixed(2));
  }

  private calculateDeliveryFee(distanceKm: number) {
    const extraDistanceKm = Math.max(distanceKm - env.DELIVERY_FREE_DISTANCE_KM, 0);
    const fee =
      env.DELIVERY_BASE_FEE + extraDistanceKm * env.DELIVERY_FEE_PER_KM;

    return Math.round(fee);
  }
}
