import { Prisma, WalletOwnerType, WalletTopupStatus, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";
import crypto from "node:crypto";
import { env } from "../../config/env.config";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { prisma } from "../../prisma/prisma.client";
import { ApiError } from "../../utils/apiError";
import {
  CreateTopupPaymentUrlResponseDto,
  ListTransactionsQueryDto,
  OrderTransactionParamDto,
  NegativeWalletListResponseDto,
  ReferenceTransactionParamDto,
  TopupListResponseDto,
  TopupQueryDto,
  WalletResponseDto,
  WalletTopupResponseDto,
  WalletTransactionResponseDto,
  WalletWithTransactionsResponseDto,
} from "./wallet.dto";
import { toWalletResponseDto, toWalletTopupResponseDto, toWalletTransactionResponseDto } from "./wallet.mapper";
import { WalletRepository } from "./wallet.repository";

type VnpayQuery = Record<string, string | string[] | undefined>;

export class WalletService {
  constructor(private readonly walletRepository: WalletRepository) {}

  private async ensureWallet(ownerType: WalletOwnerType, ownerId: string) {
    return this.walletRepository.upsertWallet(ownerType, ownerId);
  }

  async getMyWallet(ownerType: WalletOwnerType, ownerId: string): Promise<WalletResponseDto> {
    const wallet = await this.ensureWallet(ownerType, ownerId);

    return toWalletResponseDto(wallet);
  }

  async getMyTransactions(
    ownerType: WalletOwnerType,
    ownerId: string,
    query: ListTransactionsQueryDto,
  ): Promise<WalletWithTransactionsResponseDto> {
    const wallet = await this.ensureWallet(ownerType, ownerId);
    const { items, total } = await this.walletRepository.findTransactionsByWalletId(
      wallet.id,
      query.page,
      query.limit,
    );

    return {
      wallet: toWalletResponseDto(wallet),
      transactions: items.map(toWalletTransactionResponseDto),
      meta: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async getMyTransactionsByReference(
    ownerType: WalletOwnerType,
    ownerId: string,
    reference: ReferenceTransactionParamDto,
    query: ListTransactionsQueryDto,
  ): Promise<WalletWithTransactionsResponseDto> {
    const wallet = await this.ensureWallet(ownerType, ownerId);

    const { items, total } = await this.walletRepository.findTransactionsByReference(
      wallet.id,
      reference.referenceType,
      reference.referenceId,
      query.page,
      query.limit,
    );

    return {
      wallet: toWalletResponseDto(wallet),
      transactions: items.map(toWalletTransactionResponseDto),
      meta: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async getMyTransactionsByOrderId(
    ownerType: WalletOwnerType,
    ownerId: string,
    order: OrderTransactionParamDto,
    query: ListTransactionsQueryDto,
  ): Promise<WalletWithTransactionsResponseDto> {
    return this.getMyTransactionsByReference(
      ownerType,
      ownerId,
      {
        referenceType: "order",
        referenceId: order.orderId,
      },
      query,
    );
  }

  async createTopupPaymentUrl(
    ownerType: WalletOwnerType,
    ownerId: string,
    ipAddress: string,
    payload: { amount: number; bankCode?: string },
  ): Promise<CreateTopupPaymentUrlResponseDto> {
    if (payload.amount <= 0) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Topup amount must be greater than 0");
    }

    const wallet = await this.walletRepository.upsertWallet(ownerType, ownerId);
    const requestCode = this.generateRequestCode();
    const expiresAt = new Date(Date.now() + env.VNPAY_EXPIRE_MINUTES * 60_000);
    const amount = Math.round(payload.amount * 100);

    const topup = await this.walletRepository.createTopupRequest({
      wallet: {
        connect: {
          id: wallet.id,
        },
      },
      ownerType,
      ownerId,
      requestCode,
      amount: payload.amount,
      provider: "VNPAY",
      bankCode: payload.bankCode ?? null,
      status: WalletTopupStatus.PROCESSING,
      expiresAt,
      paymentData: {
        provider: "VNPAY",
        bankCode: payload.bankCode ?? null,
        state: "PROCESSING",
      },
    });

    const params: Record<string, string> = {
      vnp_Amount: String(amount),
      vnp_Command: env.VNPAY_COMMAND,
      vnp_CreateDate: this.formatVnpDate(new Date()),
      vnp_CurrCode: env.VNPAY_CURRENCY,
      vnp_ExpireDate: this.formatVnpDate(expiresAt),
      vnp_IpAddr: ipAddress || "127.0.0.1",
      vnp_Locale: env.VNPAY_LOCALE,
      vnp_OrderInfo: `Nap tien vi ${ownerType} ${ownerId}`,
      vnp_OrderType: env.VNPAY_ORDER_TYPE,
      vnp_ReturnUrl: env.WALLET_VNPAY_RETURN_URL,
      vnp_TmnCode: env.VNPAY_TMN_CODE,
      vnp_TxnRef: requestCode,
      vnp_Version: env.VNPAY_VERSION,
    };

    if (payload.bankCode) {
      params.vnp_BankCode = payload.bankCode;
    }

    const query = this.buildSignedQuery(params);
    const paymentUrl = `${env.VNPAY_URL}?${query}`;

    return {
      topupId: topup.id,
      requestCode: topup.requestCode,
      amount: payload.amount,
      expiresAt: expiresAt.toISOString(),
      paymentUrl,
    };
  }

  async getMyTopups(
    ownerType: WalletOwnerType,
    ownerId: string,
    query: TopupQueryDto,
  ): Promise<TopupListResponseDto> {
    const wallet = await this.ensureWallet(ownerType, ownerId);
    const { items, total } = await this.walletRepository.findTopupsByOwner(
      ownerType,
      wallet.ownerId,
      query.page,
      query.limit,
    );

    return {
      items: items.map(toWalletTopupResponseDto),
      meta: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async getMyTopupById(
    ownerType: WalletOwnerType,
    ownerId: string,
    topupId: string,
  ): Promise<WalletTopupResponseDto> {
    await this.ensureWallet(ownerType, ownerId);
    const topup = await this.walletRepository.findTopupByOwnerAndId(ownerType, ownerId, topupId);
    if (!topup) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Topup request not found");
    }

    return toWalletTopupResponseDto(topup);
  }

  async handleTopupVnpayReturn(query: VnpayQuery) {
    return this.handleVnpayTopupCallback(query);
  }

  async handleTopupVnpayIpn(query: VnpayQuery) {
    return this.handleVnpayTopupCallback(query);
  }

  private async handleVnpayTopupCallback(query: VnpayQuery) {
    const secureHash = this.getQueryValue(query, "vnp_SecureHash");
    const txnRef = this.getQueryValue(query, "vnp_TxnRef");
    const responseCode = this.getQueryValue(query, "vnp_ResponseCode");
    const transactionStatus = this.getQueryValue(query, "vnp_TransactionStatus");
    const transactionId = this.getQueryValue(query, "vnp_TransactionNo") ?? null;

    if (!txnRef) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Missing transaction reference");
    }

    const topup = await this.walletRepository.findTopupByRequestCode(txnRef);

    if (!topup) {
      return { rspCode: "01", message: "Topup request not found" };
    }

    const expectedSignature = this.buildSignature(query);
    if (!secureHash || !this.timingSafeEqual(secureHash, expectedSignature)) {
      return { rspCode: "97", message: "Invalid signature", topupId: topup.id };
    }

    if (topup.status === WalletTopupStatus.COMPLETED) {
      return {
        rspCode: "00",
        message: "Topup already processed",
        topupId: topup.id,
        requestCode: topup.requestCode,
        transactionId: topup.transactionId,
        status: topup.status,
      };
    }

    const success = responseCode === "00" && transactionStatus === "00";

    if (!success) {
      await prisma.walletTopupRequest.update({
        where: { id: topup.id },
        data: {
          status: WalletTopupStatus.FAILED,
          transactionId,
          paymentData: {
            ...(this.getPaymentDataObject(topup.paymentData)),
            callback: this.normalizeCallbackQuery(query),
          } as Prisma.InputJsonValue,
        },
      });

      return {
        rspCode: responseCode ?? "99",
        message: "Topup failed",
        topupId: topup.id,
        requestCode: topup.requestCode,
        transactionId,
        status: WalletTopupStatus.FAILED,
      };
    }

    const idempotencyKey = `topup:${topup.requestCode}`;

    const result = await prisma.$transaction(async (tx) => {
      const existingTransaction = await tx.walletTransaction.findUnique({
        where: { idempotencyKey },
      });

      if (existingTransaction) {
        await tx.walletTopupRequest.update({
          where: { id: topup.id },
          data: {
            status: WalletTopupStatus.COMPLETED,
            transactionId,
            paidAt: topup.paidAt ?? new Date(),
            paymentData: {
              ...(this.getPaymentDataObject(topup.paymentData)),
              callback: this.normalizeCallbackQuery(query),
            } as Prisma.InputJsonValue,
          },
        });

        return {
          rspCode: "00",
          message: "Topup already processed",
          topupId: topup.id,
          requestCode: topup.requestCode,
          transactionId,
          status: WalletTopupStatus.COMPLETED,
        };
      }

      const wallet = await tx.wallet.findUnique({
        where: { id: topup.walletId },
        select: {
          id: true,
          balance: true,
          negativeSince: true,
        },
      });

      if (!wallet) {
        throw new ApiError(HTTP_STATUS.NOT_FOUND, "Wallet not found");
      }

      const amount = this.roundMoney(Number(topup.amount));
      const currentBalance = this.roundMoney(Number(wallet.balance));
      const nextBalance = this.roundMoney(currentBalance + amount);
      const nextNegativeSince = nextBalance < 0 ? wallet.negativeSince ?? new Date() : null;

      await tx.walletTopupRequest.update({
        where: { id: topup.id },
        data: {
          status: WalletTopupStatus.COMPLETED,
          transactionId,
          paidAt: new Date(),
          paymentData: {
            ...(this.getPaymentDataObject(topup.paymentData)),
            callback: this.normalizeCallbackQuery(query),
          } as Prisma.InputJsonValue,
        },
      });

      await tx.walletTransaction.create({
        data: {
          walletId: wallet.id,
          type: WalletTransactionType.TOPUP,
          amount: new Prisma.Decimal(amount),
          balanceBefore: new Prisma.Decimal(currentBalance),
          balanceAfter: new Prisma.Decimal(nextBalance),
          referenceId: topup.id,
          referenceType: "wallet_topup",
          referenceCode: topup.requestCode,
          description: `Wallet topup ${topup.requestCode}`,
          status: WalletTransactionStatus.COMPLETED,
          idempotencyKey,
          metadata: {
            ownerType: topup.ownerType,
            ownerId: topup.ownerId,
            provider: topup.provider,
            bankCode: topup.bankCode,
            amount,
          } as Prisma.InputJsonValue,
        },
      });

      await tx.wallet.update({
        where: { id: wallet.id },
        data: {
          balance: new Prisma.Decimal(nextBalance),
          negativeSince: nextNegativeSince,
        },
      });

      return {
        rspCode: "00",
        message: "Topup successful",
        topupId: topup.id,
        requestCode: topup.requestCode,
        transactionId,
        status: WalletTopupStatus.COMPLETED,
      };
    });

    return result;
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

  private buildSignature(query: Record<string, string | string[] | undefined>) {
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

  private getQueryValue(query: Record<string, string | string[] | undefined>, key: string) {
    const value = query[key];
    if (Array.isArray(value)) {
      return value[0];
    }

    return value ?? undefined;
  }

  private normalizeCallbackQuery(query: Record<string, string | string[] | undefined>) {
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

  private getPaymentDataObject(paymentData: unknown) {
    if (paymentData && typeof paymentData === "object" && !Array.isArray(paymentData)) {
      return paymentData as Record<string, unknown>;
    }

    return {};
  }

  private generateRequestCode() {
    return `WT-${crypto.randomUUID().replace(/-/g, "").slice(0, 24).toUpperCase()}`;
  }

  async getNegativeWallets(
    query: ListTransactionsQueryDto,
  ): Promise<NegativeWalletListResponseDto> {
    const { items, total } = await this.walletRepository.findNegativeWallets(
      query.page,
      query.limit,
    );

    return {
      items: items.map((wallet) => ({
        ...toWalletResponseDto(wallet),
        negativeDays: this.getNegativeDays(wallet.negativeSince),
      })),
      meta: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  private getNegativeDays(negativeSince: Date | null) {
    if (!negativeSince) {
      return 0;
    }

    const diffMs = Date.now() - negativeSince.getTime();
    return Math.max(0, Math.floor(diffMs / (1000 * 60 * 60 * 24)));
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
