import { Prisma, PaymentMethod } from "@prisma/client";
import { HTTP_STATUS } from "../../constants/httpStatus";
import {
  DeliveryServiceClient,
} from "../../integrations/delivery.service";
import {
  MerchantAddress,
  UserAddress,
  UserServiceClient,
} from "../../integrations/user.service";
import { ApiError } from "../../utils/apiError";
import { CartService } from "../cart/cart.service";
import { DeliveryMilestoneEventPayload } from "../events/order.events";
import { VoucherRepository } from "../voucher/voucher.repository";
import { VoucherService } from "../voucher/voucher.service";
import {
  toCheckoutPreviewResponseDto,
  toCreateOrderResponseDto,
  toMyOrdersResponseDto,
  toOrderDetailResponseDto,
} from "./order.mapper";
import {
  CancelOrderDto,
  CheckoutPreviewDto,
  CheckoutPreviewResponseDto,
  CreateOrderDto,
  CreateOrderResponseDto,
  MyOrdersQueryDto,
  MyOrdersResponseDto,
  OrderDetailResponseDto,
  UpdateOrderStatusDto,
} from "./order.dto";
import { OrderRepository } from "./order.repository";

export class OrderService {
  private readonly voucherService = new VoucherService(new VoucherRepository());
  private readonly deliveryServiceClient = new DeliveryServiceClient();

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
    const context = await this.buildCheckoutContext(userId, token, payload);

    return toCheckoutPreviewResponseDto({
      userId,
      cart: context.cart,
      paymentMethod: payload.paymentMethod,
      userAddress: context.userAddress,
      merchantAddress: context.merchantAddress,
      deliveryFee: context.deliveryFee,
      distanceKm: context.distanceKm,
      estimatedTimeMinutes: context.estimatedTimeMinutes,
      deliveryFeeCurrency: context.deliveryFeeCurrency,
      isWithinDeliveryRadius: context.isWithinDeliveryRadius,
      maxDeliveryDistanceKm: context.maxDeliveryDistanceKm,
      voucherResult: context.voucherResult,
    });
  }

  async createOrder(
    userId: string,
    token: string,
    payload: CreateOrderDto,
  ): Promise<CreateOrderResponseDto> {
    const context = await this.buildCheckoutContext(userId, token, payload);
    const orderNumber = this.generateOrderNumber();

    if (!context.userAddress.recipientName || !context.userAddress.phone) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Selected delivery address must include recipient name and phone",
      );
    }

    const order = await this.orderRepository.createOrder({
      orderNumber,
      userId,
      merchantId: payload.merchantId,
      merchantName: context.merchant.storeName,
      merchantAvatar: context.merchant.storeLogoUrl ?? null,
      deliveryAddress: context.userAddress.addressLine,
      deliveryWard: context.userAddress.ward ?? null,
      deliveryDistrict: context.userAddress.district ?? null,
      deliveryCity: context.userAddress.city ?? null,
      deliveryLat: Number(context.userAddress.lat),
      deliveryLng: Number(context.userAddress.lng),
      recipientName: context.userAddress.recipientName,
      recipientPhone: context.userAddress.phone,
      subtotal: context.cart.subtotal,
      deliveryFee: context.voucherResult
        ? context.voucherResult.finalDeliveryFee
        : context.deliveryFee,
      discountAmount: context.voucherResult?.discountAmount ?? 0,
      totalAmount: context.voucherResult
        ? context.voucherResult.finalTotal
        : context.cart.subtotal + context.deliveryFee,
      paymentMethod: payload.paymentMethod as PaymentMethod,
      note: payload.note ?? null,
      voucherId: context.voucherResult?.voucher.id ?? null,
      items: context.cart.items.map((item) => ({
        productId: item.productId,
        productName: item.productName,
        productImage: item.productImage,
        unitPrice: item.unitPrice,
        selectedOptions: item.selectedOptions as Prisma.InputJsonValue,
        quantity: item.quantity,
        note: item.note,
      })),
      voucherUsage: context.voucherResult
        ? {
            voucherId: context.voucherResult.voucher.id,
            userId,
            discountAmount: context.voucherResult.discountAmount,
          }
        : null,
      orderCompletedEvent: {
        orderNumber,
        merchantId: context.merchant.id,
        merchantName: context.merchant.storeName,
        merchantAddress: {
          addressLine: context.merchantAddress.addressLine,
          lat: Number(context.merchantAddress.lat ?? 0),
          lng: Number(context.merchantAddress.lng ?? 0),
        },
        userId,
        customerName: context.userAddress.recipientName ?? "",
        customerPhone: context.userAddress.phone ?? "",
        deliveryAddress: {
          addressLine: context.userAddress.addressLine,
          lat: Number(context.userAddress.lat ?? 0),
          lng: Number(context.userAddress.lng ?? 0),
        },
        totalAmount: context.voucherResult
          ? context.voucherResult.finalTotal
          : context.cart.subtotal + context.deliveryFee,
        paymentMethod: payload.paymentMethod as PaymentMethod,
        note: payload.note ?? null,
      },
    });

    await this.cartService.clearCartByMerchant(userId, payload.merchantId);

    return toCreateOrderResponseDto({
      id: order.id,
      orderNumber: order.orderNumber,
      userId: order.userId,
      merchantId: order.merchantId,
      paymentMethod: order.paymentMethod,
      paymentStatus: order.paymentStatus,
      status: order.status,
      subtotal: Number(order.subtotal),
      deliveryFee: Number(order.deliveryFee),
      discountAmount: Number(order.discountAmount),
      totalAmount: Number(order.totalAmount),
      voucherId: order.voucherId,
      createdAt: order.createdAt,
      items: context.cart.items,
    });
  }

  async getMyOrders(
    userId: string,
    query: MyOrdersQueryDto,
  ): Promise<MyOrdersResponseDto> {
    const { items, totalCount } = await this.orderRepository.findMyOrders(
      userId,
      query,
    );

    return toMyOrdersResponseDto(items, {
      totalCount,
      page: query.page,
      limit: query.limit,
    });
  }

  async getOrderById(
    userId: string,
    orderId: string,
  ): Promise<OrderDetailResponseDto> {
    const order = await this.orderRepository.findByIdForUser(orderId, userId);

    if (!order) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    return toOrderDetailResponseDto(order);
  }

  async cancelMyOrder(
    userId: string,
    orderId: string,
    payload: CancelOrderDto,
  ): Promise<OrderDetailResponseDto> {
    const existingOrder = await this.orderRepository.findByIdForUser(orderId, userId);

    if (!existingOrder) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    this.assertCustomerCanCancel(existingOrder.status);

    const updatedOrder = await this.orderRepository.updateOrderStatus({
      orderId,
      status: "CANCELLED",
      cancelReason: payload.cancelReason,
      cancelledBy: "CUSTOMER",
      createdBy: userId,
    });

    return toOrderDetailResponseDto(updatedOrder);
  }

  async getMerchantOrders(
    merchantId: string,
    query: MyOrdersQueryDto,
  ): Promise<MyOrdersResponseDto> {
    const { items, totalCount } = await this.orderRepository.findMerchantOrders(
      merchantId,
      query,
    );

    return toMyOrdersResponseDto(items, {
      totalCount,
      page: query.page,
      limit: query.limit,
    });
  }

  async getMerchantOrderById(
    merchantId: string,
    orderId: string,
  ): Promise<OrderDetailResponseDto> {
    const order = await this.orderRepository.findByIdForMerchant(orderId, merchantId);

    if (!order) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    return toOrderDetailResponseDto(order);
  }

  async updateMerchantOrderStatus(
    merchantId: string,
    actorId: string,
    orderId: string,
    payload: UpdateOrderStatusDto,
  ): Promise<OrderDetailResponseDto> {
    const existingOrder = await this.orderRepository.findByIdForMerchant(orderId, merchantId);

    if (!existingOrder) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    this.assertMerchantStatusTransition(existingOrder.status, payload.status);

    const updatedOrder = await this.orderRepository.updateOrderStatus({
      orderId,
      status: payload.status,
      note: payload.note ?? null,
      cancelReason: payload.cancelReason ?? null,
      cancelledBy: payload.status === "CANCELLED" ? "MERCHANT" : null,
      createdBy: actorId,
    });

    return toOrderDetailResponseDto(updatedOrder);
  }

  async handleDeliveryMilestone(payload: DeliveryMilestoneEventPayload) {
    const orderId = payload.OrderId ?? payload.orderId;

    if (!orderId) {
      return;
    }

    const milestone = (payload.Milestone ?? payload.milestone ?? "").toLowerCase();

    if (!milestone) {
      return;
    }

    const existingOrder = await this.orderRepository.findById(orderId);

    if (!existingOrder || existingOrder.status === "CANCELLED" || existingOrder.status === "DELIVERED") {
      return;
    }

    const shipperId = payload.ShipperId ?? payload.shipperId ?? existingOrder.userId;
    const note = payload.Note ?? payload.note ?? null;

    if (milestone === "pickedup") {
      if (!["CONFIRMED", "PREPARING", "READY", "PICKED_UP", "DELIVERING"].includes(existingOrder.status)) {
        return;
      }

      if (existingOrder.status === "PICKED_UP" || existingOrder.status === "DELIVERING") {
        return;
      }

      await this.orderRepository.updateOrderStatus({
        orderId,
        status: "PICKED_UP",
        note: note ?? "Order picked up by shipper",
        createdBy: shipperId,
      });
      return;
    }

    if (milestone === "delivered") {
      if (!["PICKED_UP", "DELIVERING", "READY"].includes(existingOrder.status)) {
        return;
      }

      await this.orderRepository.updateOrderStatus({
        orderId,
        status: "DELIVERED",
        note: note ?? "Order delivered successfully",
        createdBy: shipperId,
      });
    }
  }

  private async buildCheckoutContext(
    userId: string,
    token: string,
    payload: CheckoutPreviewDto | CreateOrderDto,
  ) {
    const cart = await this.cartService.getCartByMerchant(userId, payload.merchantId);

    if (cart.items.length === 0) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Cannot continue because the merchant cart is empty",
      );
    }

    const [userAddress, merchantAddress, merchant] = await Promise.all([
      this.userServiceClient.getUserAddressById(userId, payload.addressId, token),
      this.userServiceClient.getMerchantPrimaryAddress(payload.merchantId),
      this.userServiceClient.getMerchantById(payload.merchantId),
    ]);

    this.assertAddressCoordinates(userAddress, merchantAddress);

    const estimate = await this.deliveryServiceClient.estimateDeliveryFee(
      {
        pickupLat: Number(merchantAddress.lat),
        pickupLng: Number(merchantAddress.lng),
        deliveryLat: Number(userAddress.lat),
        deliveryLng: Number(userAddress.lng),
      },
      token,
    );
    const distanceKm = estimate.distanceKm;
    const deliveryFee = estimate.deliveryFee;

    const voucherResult = payload.voucherCode
      ? await this.voucherService.validateVoucher({
          code: payload.voucherCode,
          userId,
          merchantId: payload.merchantId,
          subtotal: cart.subtotal,
          deliveryFee,
        })
      : null;

    return {
      cart,
      userAddress,
      merchantAddress,
      merchant,
      distanceKm,
      deliveryFee,
      estimatedTimeMinutes: estimate.estimatedTimeMinutes,
      deliveryFeeCurrency: estimate.currency,
      isWithinDeliveryRadius: estimate.isWithinDeliveryRadius,
      maxDeliveryDistanceKm: estimate.maxDeliveryDistanceKm,
      voucherResult,
    };
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

  private generateOrderNumber() {
    const timestamp = Date.now().toString().slice(-8);
    const random = Math.floor(Math.random() * 10000)
      .toString()
      .padStart(4, "0");

    return `FD${timestamp}${random}`;
  }

  private assertCustomerCanCancel(currentStatus: string) {
    const cancellableStatuses = new Set(["PENDING", "CONFIRMED"]);

    if (!cancellableStatuses.has(currentStatus)) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Order can only be cancelled when it is pending or confirmed",
      );
    }
  }

  private assertMerchantStatusTransition(currentStatus: string, nextStatus: string) {
    if (currentStatus === "CANCELLED") {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Cancelled orders cannot be updated",
      );
    }

    if (currentStatus === "DELIVERED") {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Delivered orders cannot be updated",
      );
    }

    const transitionMap: Record<string, string[]> = {
      PENDING: ["CONFIRMED", "CANCELLED"],
      CONFIRMED: ["PREPARING", "CANCELLED"],
      PREPARING: ["READY", "CANCELLED"],
      READY: [],
      PICKED_UP: [],
      DELIVERING: [],
      DELIVERED: [],
      CANCELLED: [],
    };

    const allowedStatuses = transitionMap[currentStatus] ?? [];

    if (!allowedStatuses.includes(nextStatus)) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        `Cannot update order status from ${currentStatus} to ${nextStatus}`,
      );
    }
  }
}
