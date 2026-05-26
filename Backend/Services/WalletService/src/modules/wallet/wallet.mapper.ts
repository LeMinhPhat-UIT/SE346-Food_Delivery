import { WalletOwnerType, WalletTopupStatus, WalletTransactionStatus, WalletTransactionType } from "@prisma/client";
import { WalletResponseDto, WalletTopupResponseDto, WalletTransactionResponseDto } from "./wallet.dto";

type WalletLike = {
  id: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  balance: { toNumber: () => number } | number;
  negativeSince: Date | null;
  currency: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
};

type WalletTopupLike = {
  id: string;
  walletId: string;
  ownerType: WalletOwnerType;
  ownerId: string;
  requestCode: string;
  amount: { toNumber: () => number } | number;
  provider: string;
  bankCode: string | null;
  status: WalletTopupStatus;
  transactionId: string | null;
  paymentData: unknown;
  expiresAt: Date;
  paidAt: Date | null;
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
  negativeSince: wallet.negativeSince ? wallet.negativeSince.toISOString() : null,
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

export const toWalletTopupResponseDto = (
  topup: WalletTopupLike,
): WalletTopupResponseDto => ({
  id: topup.id,
  walletId: topup.walletId,
  ownerType: topup.ownerType,
  ownerId: topup.ownerId,
  requestCode: topup.requestCode,
  amount: toAmount(topup.amount),
  provider: topup.provider,
  bankCode: topup.bankCode,
  status: topup.status,
  transactionId: topup.transactionId,
  paymentData: topup.paymentData ?? null,
  expiresAt: topup.expiresAt.toISOString(),
  paidAt: topup.paidAt ? topup.paidAt.toISOString() : null,
  createdAt: topup.createdAt.toISOString(),
  updatedAt: topup.updatedAt.toISOString(),
});
