import { Prisma, WalletOwnerType } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";

const walletSelect = {
  id: true,
  ownerType: true,
  ownerId: true,
  balance: true,
  currency: true,
  isActive: true,
  createdAt: true,
  updatedAt: true,
} as const;

const transactionSelect = {
  id: true,
  walletId: true,
  type: true,
  amount: true,
  balanceBefore: true,
  balanceAfter: true,
  referenceId: true,
  referenceType: true,
  referenceCode: true,
  description: true,
  status: true,
  idempotencyKey: true,
  metadata: true,
  createdAt: true,
  updatedAt: true,
} as const;

export class WalletRepository {
  async findByOwner(ownerType: WalletOwnerType, ownerId: string) {
    return prisma.wallet.findUnique({
      where: {
        ownerType_ownerId: {
          ownerType,
          ownerId,
        },
      },
      select: walletSelect,
    });
  }

  async findTransactionsByWalletId(walletId: string, page: number, limit: number) {
    const [items, total] = await prisma.$transaction([
      prisma.walletTransaction.findMany({
        where: { walletId },
        orderBy: { createdAt: "desc" },
        skip: (page - 1) * limit,
        take: limit,
        select: transactionSelect,
      }),
      prisma.walletTransaction.count({
        where: { walletId },
      }),
    ]);

    return { items, total };
  }

  async findTransactionsByOwner(ownerType: WalletOwnerType, ownerId: string, page: number, limit: number) {
    const wallet = await this.findByOwner(ownerType, ownerId);

    if (!wallet) {
      return {
        wallet: null,
        items: [],
        total: 0,
      };
    }

    const { items, total } = await this.findTransactionsByWalletId(wallet.id, page, limit);

    return { wallet, items, total };
  }

  async createWallet(ownerType: WalletOwnerType, ownerId: string) {
    return prisma.wallet.create({
      data: {
        ownerType,
        ownerId,
      },
      select: walletSelect,
    });
  }

  async upsertWallet(ownerType: WalletOwnerType, ownerId: string) {
    const existing = await this.findByOwner(ownerType, ownerId);
    if (existing) {
      return existing;
    }

    return this.createWallet(ownerType, ownerId);
  }

  async createTransaction(
    walletId: string,
    data: Prisma.WalletTransactionCreateInput,
  ) {
    return prisma.walletTransaction.create({
      data,
      select: transactionSelect,
    });
  }
}
