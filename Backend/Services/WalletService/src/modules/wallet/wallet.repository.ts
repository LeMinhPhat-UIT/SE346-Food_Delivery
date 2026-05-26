import { Prisma, WalletOwnerType } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";

const walletSelect = {
  id: true,
  ownerType: true,
  ownerId: true,
  balance: true,
  negativeSince: true,
  currency: true,
  isActive: true,
  createdAt: true,
  updatedAt: true,
} as const;

const topupSelect = {
  id: true,
  walletId: true,
  ownerType: true,
  ownerId: true,
  requestCode: true,
  amount: true,
  provider: true,
  bankCode: true,
  status: true,
  transactionId: true,
  paymentData: true,
  expiresAt: true,
  paidAt: true,
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

  async findTransactionsByReference(
    walletId: string,
    referenceType: string,
    referenceId: string,
    page: number,
    limit: number,
  ) {
    const [items, total] = await prisma.$transaction([
      prisma.walletTransaction.findMany({
        where: {
          walletId,
          referenceType,
          referenceId,
        },
        orderBy: { createdAt: "desc" },
        skip: (page - 1) * limit,
        take: limit,
        select: transactionSelect,
      }),
      prisma.walletTransaction.count({
        where: {
          walletId,
          referenceType,
          referenceId,
        },
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

  async findTopupByRequestCode(requestCode: string) {
    return prisma.walletTopupRequest.findUnique({
      where: { requestCode },
      select: topupSelect,
    });
  }

  async findTopupById(topupId: string) {
    return prisma.walletTopupRequest.findUnique({
      where: { id: topupId },
      select: topupSelect,
    });
  }

  async findTopupsByOwner(ownerType: WalletOwnerType, ownerId: string, page: number, limit: number) {
    const [items, total] = await prisma.$transaction([
      prisma.walletTopupRequest.findMany({
        where: {
          ownerType,
          ownerId,
        },
        orderBy: { createdAt: "desc" },
        skip: (page - 1) * limit,
        take: limit,
        select: topupSelect,
      }),
      prisma.walletTopupRequest.count({
        where: {
          ownerType,
          ownerId,
        },
      }),
    ]);

    return { items, total };
  }

  async findTopupByOwnerAndId(ownerType: WalletOwnerType, ownerId: string, topupId: string) {
    return prisma.walletTopupRequest.findFirst({
      where: {
        id: topupId,
        ownerType,
        ownerId,
      },
      select: topupSelect,
    });
  }

  async createTopupRequest(data: Prisma.WalletTopupRequestCreateInput) {
    return prisma.walletTopupRequest.create({
      data,
      select: topupSelect,
    });
  }

  async updateTopupRequest(id: string, data: Prisma.WalletTopupRequestUpdateInput) {
    return prisma.walletTopupRequest.update({
      where: { id },
      data,
      select: topupSelect,
    });
  }

  async updateWalletBalance(walletId: string, balance: number, negativeSince: Date | null) {
    return prisma.wallet.update({
      where: { id: walletId },
      data: {
        balance,
        negativeSince,
      },
      select: walletSelect,
    });
  }

  async findNegativeWallets(page: number, limit: number) {
    const [items, total] = await prisma.$transaction([
      prisma.wallet.findMany({
        where: {
          balance: {
            lt: 0,
          },
        },
        orderBy: [{ negativeSince: "asc" }, { updatedAt: "desc" }],
        skip: (page - 1) * limit,
        take: limit,
        select: walletSelect,
      }),
      prisma.wallet.count({
        where: {
          balance: {
            lt: 0,
          },
        },
      }),
    ]);

    return { items, total };
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
