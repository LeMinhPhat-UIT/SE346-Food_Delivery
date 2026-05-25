import { Wallet, WalletTransaction } from "@prisma/client";
import { WalletResponseDto, WalletTransactionResponseDto } from "./wallet.dto";

export const toWalletResponseDto = (wallet: Wallet): WalletResponseDto => ({
  id: wallet.id,
  ownerType: wallet.ownerType,
  ownerId: wallet.ownerId,
  balance: Number(wallet.balance),
  currency: wallet.currency,
  isActive: wallet.isActive,
  createdAt: wallet.createdAt.toISOString(),
  updatedAt: wallet.updatedAt.toISOString(),
});

export const toWalletTransactionResponseDto = (
  tx: WalletTransaction,
): WalletTransactionResponseDto => ({
  id: tx.id,
  walletId: tx.walletId,
  type: tx.type,
  amount: Number(tx.amount),
  balanceBefore: Number(tx.balanceBefore),
  balanceAfter: Number(tx.balanceAfter),
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
