import { z } from "zod";
import {
  listTransactionsQuerySchema,
  orderTransactionParamSchema,
  ownerParamSchema,
  ownerTypeParamSchema,
  referenceTransactionParamSchema,
  topupBodySchema,
  topupParamSchema,
  topupQuerySchema,
} from "./wallet.schema";
import { WalletOwnerType, WalletTopupStatus, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";

export type OwnerTypeParamDto = z.infer<typeof ownerTypeParamSchema>;
export type OwnerParamDto = z.infer<typeof ownerParamSchema>;
export type ListTransactionsQueryDto = z.infer<typeof listTransactionsQuerySchema>;
export type ReferenceTransactionParamDto = z.infer<typeof referenceTransactionParamSchema>;
export type OrderTransactionParamDto = z.infer<typeof orderTransactionParamSchema>;
export type TopupBodyDto = z.infer<typeof topupBodySchema>;
export type TopupParamDto = z.infer<typeof topupParamSchema>;
export type TopupQueryDto = z.infer<typeof topupQuerySchema>;

export type WalletResponseDto = {
  id: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  balance: number;
  negativeSince: string | null;
  currency: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type WalletTransactionResponseDto = {
  id: string;
  walletId: string;
  type: WalletTransactionType;
  amount: number;
  balanceBefore: number;
  balanceAfter: number;
  referenceId: string | null;
  referenceType: string | null;
  referenceCode: string | null;
  description: string | null;
  status: WalletTransactionStatus;
  idempotencyKey: string | null;
  metadata: unknown;
  createdAt: string;
  updatedAt: string;
};

export type WalletWithTransactionsResponseDto = {
  wallet: WalletResponseDto;
  transactions: WalletTransactionResponseDto[];
  meta: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
};

export type NegativeWalletResponseDto = WalletResponseDto & {
  negativeDays: number;
};

export type NegativeWalletListResponseDto = {
  items: NegativeWalletResponseDto[];
  meta: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
};

export type WalletTopupResponseDto = {
  id: string;
  walletId: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  requestCode: string;
  amount: number;
  provider: string;
  bankCode: string | null;
  status: WalletTopupStatus;
  transactionId: string | null;
  paymentData: unknown;
  expiresAt: string;
  paidAt: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CreateTopupPaymentUrlResponseDto = {
  topupId: string;
  requestCode: string;
  amount: number;
  expiresAt: string;
  paymentUrl: string;
};

export type TopupListResponseDto = {
  items: WalletTopupResponseDto[];
  meta: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
};
