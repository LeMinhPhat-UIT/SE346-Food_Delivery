import { WalletOwnerType, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";
import { WalletResponseDto, WalletTransactionResponseDto } from "./wallet.dto";

type WalletLike = {
  id: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  balance: { toNumber: () => number } | number;
  currency: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
};

type WalletTransactionLike = {
  id: string;
  walletId: string;
  type: WalletTransactionType;
  amount: { toNumber: () => number } | number;
  balanceBefore: { toNumber: () => number } | number;
  balanceAfter: { toNumber: () => number } | number;
  referenceId: string | null;
  referenceType: string | null;
  referenceCode: string | null;
  description: string | null;
  status: WalletTransactionStatus;
  idempotencyKey: string | null;
  metadata: unknown;
  createdAt: Date;
  updatedAt: Date;
};

const toAmount = (value: { toNumber: () => number } | number) =>
  typeof value === "number" ? value : value.toNumber();

export const toWalletResponseDto = (wallet: WalletLike): WalletResponseDto => ({
  id: wallet.id,
  ownerType: wallet.ownerType,
  ownerId: wallet.ownerId,
  balance: toAmount(wallet.balance),
  currency: wallet.currency,
  isActive: wallet.isActive,
  createdAt: wallet.createdAt.toISOString(),
  updatedAt: wallet.updatedAt.toISOString(),
});

export const toWalletTransactionResponseDto = (
  tx: WalletTransactionLike,
): WalletTransactionResponseDto => ({
  id: tx.id,
  walletId: tx.walletId,
  type: tx.type,
  amount: toAmount(tx.amount),
  balanceBefore: toAmount(tx.balanceBefore),
  balanceAfter: toAmount(tx.balanceAfter),
  referenceId: tx.referenceId,
  referenceType: tx.referenceType,
  referenceCode: tx.referenceCode,
  description: tx.description,
  status: tx.status,
  idempotencyKey: tx.idempotencyKey,
  metadata: tx.metadata ?? null,
  createdAt: tx.createdAt.toISOString(),
  updatedAt: tx.updatedAt.toISOString(),
});
