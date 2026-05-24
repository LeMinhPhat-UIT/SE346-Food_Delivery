import crypto from "crypto";
import { OrderPaymentStatus, PaymentMethod, PaymentTransactionStatus, Prisma } from "@prisma/client";
import { env } from "../../config/env.config";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { prisma } from "../../prisma/prisma.client";
import { ApiError } from "../../utils/apiError";
import { OrderRepository } from "../order/order.repository";
import { toPaymentDetailResponseDto, isSuccessfulVnpayResult } from "./payment.mapper";
import {
  CreateVnpayPaymentUrlDto,
  CreateVnpayPaymentUrlResponseDto,
  PaymentDetailResponseDto,
  VnpayCallbackResponseDto,
} from "./payment.dto";
import { PaymentRepository } from "./payment.repository";

type VnpayQuery = Record<string, string | string[] | undefined>;

export class PaymentService {
  constructor(
    private readonly paymentRepository: PaymentRepository,
    private readonly orderRepository: OrderRepository,
  ) {}

  async getPaymentByOrderId(userId: string, orderId: string): Promise<PaymentDetailResponseDto> {
    const order = await this.orderRepository.findByIdForUser(orderId, userId);

    if (!order) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    const payment = await this.paymentRepository.findByOrderId(orderId);

    if (!payment) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Payment not found");
    }

    return toPaymentDetailResponseDto(payment);
  }

  async createVnpayPaymentUrl(
    userId: string,
    orderId: string,
    ipAddress: string,
    payload: CreateVnpayPaymentUrlDto,
  ): Promise<CreateVnpayPaymentUrlResponseDto> {
    const order = await this.orderRepository.findByIdForUser(orderId, userId);

    if (!order) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Order not found");
    }

    if (order.paymentMethod !== PaymentMethod.VNPAY) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "This order does not use VNPay");
    }

    if (order.paymentStatus === OrderPaymentStatus.PAID) {
      throw new ApiError(HTTP_STATUS.CONFLICT, "This order has already been paid");
    }

    const payment = await this.paymentRepository.findByOrderId(orderId);

    if (!payment) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Payment record not found");
    }

    await this.paymentRepository.updateById(payment.id, {
      status: PaymentTransactionStatus.PROCESSING,
      paymentData: {
        ...(this.getPaymentDataObject(payment.paymentData)),
        provider: "VNPAY",
        bankCode: payload.bankCode ?? null,
        state: "PROCESSING",
      } as Prisma.InputJsonValue,
    });

    const amount = Math.round(Number(order.totalAmount) * 100);
    const expiresAt = new Date(Date.now() + env.VNPAY_EXPIRE_MINUTES * 60_000);
    const params: Record<string, string> = {
      vnp_Amount: String(amount),
      vnp_Command: env.VNPAY_COMMAND,
      vnp_CreateDate: this.formatVnpDate(new Date()),
      vnp_CurrCode: env.VNPAY_CURRENCY,
      vnp_ExpireDate: this.formatVnpDate(expiresAt),
      vnp_IpAddr: ipAddress || "127.0.0.1",
      vnp_Locale: env.VNPAY_LOCALE,
      vnp_OrderInfo: `Thanh toan don hang ${order.orderNumber}`,
      vnp_OrderType: env.VNPAY_ORDER_TYPE,
      vnp_ReturnUrl: env.VNPAY_RETURN_URL,
      vnp_TmnCode: env.VNPAY_TMN_CODE,
      vnp_TxnRef: order.orderNumber,
      vnp_Version: env.VNPAY_VERSION,
    };

    if (payload.bankCode) {
      params.vnp_BankCode = payload.bankCode;
    }

    const query = this.buildSignedQuery(params);
    const paymentUrl = `${env.VNPAY_URL}?${query}`;

    return {
      orderId: order.id,
      orderNumber: order.orderNumber,
      amount: Number(order.totalAmount),
      expiresAt: expiresAt.toISOString(),
      paymentUrl,
    };
  }

  async handleVnpayReturn(query: VnpayQuery): Promise<VnpayCallbackResponseDto> {
    return this.processVnpayCallback(query);
  }

  async handleVnpayIpn(query: VnpayQuery): Promise<VnpayCallbackResponseDto> {
    return this.processVnpayCallback(query);
  }

  private async processVnpayCallback(query: VnpayQuery): Promise<VnpayCallbackResponseDto> {
    const secureHash = this.getQueryValue(query, "vnp_SecureHash");
    const txnRef = this.getQueryValue(query, "vnp_TxnRef");
    const responseCode = this.getQueryValue(query, "vnp_ResponseCode");
    const transactionStatus = this.getQueryValue(query, "vnp_TransactionStatus");
    const transactionId = this.getQueryValue(query, "vnp_TransactionNo") ?? null;

    if (!txnRef) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Missing transaction reference");
    }

    const payment = await this.paymentRepository.findByTxnRef(txnRef);

    if (!payment) {
      return {
        rspCode: "01",
        message: "Order not found",
      };
    }

    const expectedSignature = this.buildSignature(query);
    if (!secureHash || !this.timingSafeEqual(secureHash, expectedSignature)) {
      return {
        rspCode: "97",
        message: "Invalid signature",
        orderId: payment.orderId,
        orderNumber: payment.order?.orderNumber,
      };
    }

    if (payment.status === PaymentTransactionStatus.COMPLETED && payment.order?.paymentStatus === OrderPaymentStatus.PAID) {
      return {
        rspCode: "00",
        message: "Order already processed",
        orderId: payment.orderId,
        orderNumber: payment.order?.orderNumber,
        transactionId: payment.transactionId,
        paymentStatus: payment.status,
        orderPaymentStatus: payment.order?.paymentStatus,
      };
    }

    const success = isSuccessfulVnpayResult(responseCode ?? undefined, transactionStatus ?? undefined);

    if (!success) {
      await prisma.$transaction(async (tx) => {
        await tx.payment.update({
          where: { id: payment.id },
          data: {
            status: PaymentTransactionStatus.FAILED,
            transactionId,
            paymentData: {
              ...(this.getPaymentDataObject(payment.paymentData)),
              callback: this.normalizeCallbackQuery(query),
            } as Prisma.InputJsonValue,
          },
        });

        await tx.order.update({
          where: { id: payment.orderId },
          data: {
            paymentStatus: OrderPaymentStatus.FAILED,
          },
        });
      });

      return {
        rspCode: responseCode ?? "99",
        message: "Payment failed",
        orderId: payment.orderId,
        orderNumber: payment.order?.orderNumber,
        transactionId,
        paymentStatus: PaymentTransactionStatus.FAILED,
        orderPaymentStatus: OrderPaymentStatus.FAILED,
      };
    }

    const order = await this.orderRepository.findById(payment.orderId);

    if (!order) {
      return {
        rspCode: "01",
        message: "Order not found",
      };
    }

    const orderCompletedEvent = this.extractOrderCompletedEvent(payment.paymentData);

    await prisma.$transaction(async (tx) => {
      await tx.payment.update({
        where: { id: payment.id },
        data: {
          status: PaymentTransactionStatus.COMPLETED,
          transactionId,
          paidAt: new Date(),
          paymentData: {
            ...(this.getPaymentDataObject(payment.paymentData)),
            callback: this.normalizeCallbackQuery(query),
          } as Prisma.InputJsonValue,
        },
      });

      await tx.order.update({
        where: { id: payment.orderId },
        data: {
          paymentStatus: OrderPaymentStatus.PAID,
        },
      });

      await tx.outboxMessage.create({
        data: {
          aggregateType: "Order",
          aggregateId: order.id,
          eventType: "order.completed",
          payload: orderCompletedEvent
            ? {
                OrderId: order.id,
                OrderNumber: order.orderNumber,
                OrderStatus: order.status,
                MerchantId: orderCompletedEvent.merchantId,
                MerchantStoreName: orderCompletedEvent.merchantName,
                MerchantAddress: {
                  AddressLine: orderCompletedEvent.merchantAddress.addressLine,
                  Lat: Number(orderCompletedEvent.merchantAddress.lat ?? 0),
                  Lng: Number(orderCompletedEvent.merchantAddress.lng ?? 0),
                },
                UserId: orderCompletedEvent.userId,
                CustomerName: orderCompletedEvent.customerName,
                CustomerPhone: orderCompletedEvent.customerPhone,
                DeliveryAddress: {
                  AddressLine: orderCompletedEvent.deliveryAddress.addressLine,
                  Lat: Number(orderCompletedEvent.deliveryAddress.lat ?? 0),
                  Lng: Number(orderCompletedEvent.deliveryAddress.lng ?? 0),
                },
                TotalAmount: Number(orderCompletedEvent.totalAmount ?? order.totalAmount),
                PaymentMethod: orderCompletedEvent.paymentMethod ?? order.paymentMethod,
                Note: orderCompletedEvent.note ?? order.note,
              } as Prisma.InputJsonValue
            : ({
                OrderId: order.id,
                OrderNumber: order.orderNumber,
                OrderStatus: order.status,
                MerchantId: order.merchantId,
                MerchantStoreName: order.merchantName,
                MerchantAddress: {
                  AddressLine: order.deliveryAddress,
                  Lat: Number(order.deliveryLat ?? 0),
                  Lng: Number(order.deliveryLng ?? 0),
                },
                UserId: order.userId,
                CustomerName: order.recipientName,
                CustomerPhone: order.recipientPhone,
                DeliveryAddress: {
                  AddressLine: order.deliveryAddress,
                  Lat: Number(order.deliveryLat ?? 0),
                  Lng: Number(order.deliveryLng ?? 0),
                },
                TotalAmount: Number(order.totalAmount),
                PaymentMethod: order.paymentMethod,
                Note: order.note,
              } as Prisma.InputJsonValue),
        },
      });
    });

    return {
      rspCode: "00",
      message: "Payment successful",
      orderId: payment.orderId,
      orderNumber: order.orderNumber,
      transactionId,
      paymentStatus: PaymentTransactionStatus.COMPLETED,
      orderPaymentStatus: OrderPaymentStatus.PAID,
    };
  }

  private buildSignedQuery(params: Record<string, string>) {
    const sorted = Object.keys(params)
      .sort()
      .reduce<Record<string, string>>((acc, key) => {
        acc[key] = params[key];
        return acc;
      }, {});

    const signData = Object.entries(sorted)
      .map(([key, value]) => `${this.vnpEncode(key)}=${this.vnpEncode(value)}`)
      .join("&");

    const secureHash = crypto
      .createHmac("sha512", env.VNPAY_HASH_SECRET)
      .update(Buffer.from(signData, "utf8"))
      .digest("hex");

    return `${signData}&vnp_SecureHash=${secureHash}`;
  }

  private buildSignature(query: VnpayQuery) {
    const filteredEntries = Object.entries(query)
      .filter(([key, value]) =>
        key.startsWith("vnp_") &&
        key !== "vnp_SecureHash" &&
        key !== "vnp_SecureHashType" &&
        typeof value !== "undefined" &&
        value !== null,
      )
      .map(([key, value]) => [key, Array.isArray(value) ? value[0] : String(value)] as const)
      .sort(([a], [b]) => a.localeCompare(b));

    const signData = filteredEntries
      .map(([key, value]) => `${this.vnpEncode(key)}=${this.vnpEncode(value)}`)
      .join("&");

    return crypto
      .createHmac("sha512", env.VNPAY_HASH_SECRET)
      .update(Buffer.from(signData, "utf8"))
      .digest("hex");
  }

  private vnpEncode(value: string) {
    return encodeURIComponent(value)
      .replace(/[!'()*]/g, (char) => `%${char.charCodeAt(0).toString(16).toUpperCase()}`)
      .replace(/%20/g, "+");
  }

  private timingSafeEqual(a: string, b: string) {
    const left = Buffer.from(a.toLowerCase(), "utf8");
    const right = Buffer.from(b.toLowerCase(), "utf8");

    return (
      left.length === right.length &&
      crypto.timingSafeEqual(left, right)
    );
  }

  private getQueryValue(query: VnpayQuery, key: string) {
    const value = query[key];
    if (Array.isArray(value)) {
      return value[0];
    }

    return value ?? undefined;
  }

  private normalizeCallbackQuery(query: VnpayQuery) {
    return Object.fromEntries(
      Object.entries(query).map(([key, value]) => [
        key,
        Array.isArray(value) ? value[0] : value ?? null,
      ]),
    );
  }

  private formatVnpDate(date: Date) {
    const vietnamTime = new Date(date.getTime() + 7 * 60 * 60 * 1000);
    const pad = (value: number) => String(value).padStart(2, "0");
    return [
      vietnamTime.getUTCFullYear(),
      pad(vietnamTime.getUTCMonth() + 1),
      pad(vietnamTime.getUTCDate()),
      pad(vietnamTime.getUTCHours()),
      pad(vietnamTime.getUTCMinutes()),
      pad(vietnamTime.getUTCSeconds()),
    ].join("");
  }

  private getPaymentDataObject(paymentData: Prisma.JsonValue | null) {
    if (paymentData && typeof paymentData === "object" && !Array.isArray(paymentData)) {
      return paymentData as Record<string, unknown>;
    }

    return {};
  }

  private extractOrderCompletedEvent(paymentData: Prisma.JsonValue | null) {
    const data = this.getPaymentDataObject(paymentData);
    const snapshot = data.orderCompletedEvent;

    if (!snapshot || typeof snapshot !== "object" || Array.isArray(snapshot)) {
      return null;
    }

    return snapshot as {
      orderNumber: string;
      merchantId: string;
      merchantName: string;
      merchantAddress: {
        addressLine: string;
        lat: number;
        lng: number;
      };
      userId: string;
      customerName: string;
      customerPhone: string;
      deliveryAddress: {
        addressLine: string;
        lat: number;
        lng: number;
      };
      totalAmount: number;
      paymentMethod: PaymentMethod;
      note: string | null;
    };
  }
}
