import { prisma } from "../../prisma/prisma.client";
import {
  CreateVoucherDto,
  UpdateVoucherDto,
  VoucherQueryDto,
} from "./voucher.dto";

export const voucherSelect = {
  id: true,
  code: true,
  name: true,
  description: true,
  discountType: true,
  discountValue: true,
  maxDiscount: true,
  minOrderAmount: true,
  discountTarget: true,
  merchantId: true,
  usageLimit: true,
  perUserLimit: true,
  startDate: true,
  endDate: true,
  isActive: true,
  createdAt: true,
  deletedAt: true,
  _count: {
    select: {
      usages: true,
    },
  },
} as const;

export type VoucherRecord = {
  id: string;
  code: string;
  name: string;
  description: string | null;
  discountType: "PERCENTAGE" | "FIXED";
  discountValue: number | { toString(): string };
  maxDiscount: number | { toString(): string } | null;
  minOrderAmount: number | { toString(): string } | null;
  discountTarget: "SUBTOTAL" | "DELIVERY_FEE";
  merchantId: string | null;
  usageLimit: number | null;
  perUserLimit: number;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
  createdAt: Date;
  deletedAt: Date | null;
  _count: {
    usages: number;
  };
};

type VoucherUpdatePayload = Partial<CreateVoucherDto> & {
  deletedAt?: Date | null;
  isActive?: boolean;
};

export class VoucherRepository {
  async findAll(filters: VoucherQueryDto) {
    const {
      page,
      limit,
      search,
      merchantId,
      isActive,
      includeDeleted,
      discountType,
      discountTarget,
      availability,
      sortBy,
      sortOrder,
    } = filters;

    const now = new Date();
    const where = {
      deletedAt: includeDeleted ? undefined : null,
      merchantId,
      isActive,
      discountType,
      discountTarget,
      ...(search
        ? {
            OR: [
              {
                code: {
                  contains: search,
                  mode: "insensitive" as const,
                },
              },
              {
                name: {
                  contains: search,
                  mode: "insensitive" as const,
                },
              },
              {
                description: {
                  contains: search,
                  mode: "insensitive" as const,
                },
              },
            ],
          }
        : {}),
      ...(availability === "active"
        ? {
            isActive: true,
            startDate: { lte: now },
            endDate: { gte: now },
          }
        : {}),
      ...(availability === "upcoming"
        ? {
            startDate: { gt: now },
          }
        : {}),
      ...(availability === "expired"
        ? {
            endDate: { lt: now },
          }
        : {}),
      ...(availability === "inactive"
        ? {
            isActive: false,
          }
        : {}),
    };

    const orderBy =
      sortBy === "usedCount"
        ? {
            usages: {
              _count: sortOrder,
            },
          }
        : {
            [sortBy]: sortOrder,
          };

    const [items, totalCount] = await Promise.all([
      prisma.voucher.findMany({
        where,
        orderBy,
        skip: (page - 1) * limit,
        take: limit,
        select: voucherSelect,
      }) as Promise<VoucherRecord[]>,
      prisma.voucher.count({ where }),
    ]);

    return { items, totalCount };
  }

  async findById(id: string) {
    const voucher = (await prisma.voucher.findUnique({
      where: { id },
      select: voucherSelect,
    })) as VoucherRecord | null;

    if (!voucher || voucher.deletedAt !== null) {
      return null;
    }

    return voucher;
  }

  async findByIdIncludingDeleted(id: string) {
    return (await prisma.voucher.findUnique({
      where: { id },
      select: voucherSelect,
    })) as VoucherRecord | null;
  }

  async findByCode(code: string) {
    const voucher = (await prisma.voucher.findUnique({
      where: { code },
      select: voucherSelect,
    })) as VoucherRecord | null;

    if (!voucher || voucher.deletedAt !== null) {
      return null;
    }

    return voucher;
  }

  async findByCodeIncludingDeleted(code: string) {
    return (await prisma.voucher.findUnique({
      where: { code },
      select: voucherSelect,
    })) as VoucherRecord | null;
  }

  async findByMerchantId(merchantId: string) {
    return (await prisma.voucher.findMany({
      where: {
        merchantId,
        deletedAt: null,
      },
      select: voucherSelect,
    })) as VoucherRecord[];
  }

  async create(data: CreateVoucherDto) {
    return (await prisma.voucher.create({
      data: {
        ...data,
        code: data.code.toUpperCase(),
      },
      select: voucherSelect,
    })) as VoucherRecord;
  }

  async update(id: string, data: UpdateVoucherDto | VoucherUpdatePayload) {
    const normalizedData =
      "code" in data && typeof data.code === "string"
        ? { ...data, code: data.code.toUpperCase() }
        : data;

    return (await prisma.voucher.update({
      where: { id },
      data: normalizedData,
      select: voucherSelect,
    })) as VoucherRecord;
  }

  async countTotalUsage(voucherId: string) {
    return prisma.voucherUsage.count({
      where: { voucherId },
    });
  }

  async countUserUsage(voucherId: string, userId: string) {
    return prisma.voucherUsage.count({
      where: {
        voucherId,
        userId,
      },
    });
  }
}
