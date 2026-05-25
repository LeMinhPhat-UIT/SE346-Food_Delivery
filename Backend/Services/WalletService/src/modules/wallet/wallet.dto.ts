import { z } from "zod";
import { listTransactionsQuerySchema, ownerParamSchema, ownerTypeParamSchema } from "./wallet.schema";
import { WalletOwnerType, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";

export type OwnerTypeParamDto = z.infer<typeof ownerTypeParamSchema>;
export type OwnerParamDto = z.infer<typeof ownerParamSchema>;
export type ListTransactionsQueryDto = z.infer<typeof listTransactionsQuerySchema>;

export type WalletResponseDto = {
  id: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  balance: number;
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
