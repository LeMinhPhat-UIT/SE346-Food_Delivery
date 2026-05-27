import { prisma } from "../../prisma/prisma.client";

const metricSelect = {
  metricDate: true,
  grossRevenue: true,
  netRevenue: true,
  orderCount: true,
  paidOrderCount: true,
  cancelledOrderCount: true,
  deliveryFeeTotal: true,
  discountTotal: true,
  voucherUsageCount: true,
  codOrderCount: true,
  vnpayOrderCount: true,
  uniqueCustomers: true,
  uniqueMerchants: true,
  uniqueShippers: true,
} as const;

const merchantMetricSelect = {
  metricDate: true,
  merchantId: true,
  grossRevenue: true,
  netRevenue: true,
  orderCount: true,
  paidOrderCount: true,
  cancelledOrderCount: true,
  subtotalRevenue: true,
  deliveryFeeRevenue: true,
  discountTotal: true,
  voucherUsageCount: true,
  avgOrderValue: true,
} as const;

const shipperMetricSelect = {
  metricDate: true,
  shipperId: true,
  assignedOrderCount: true,
  pickedUpOrderCount: true,
  deliveredOrderCount: true,
  cancelledOrderCount: true,
  completionRate: true,
  avgDeliveryTimeMinutes: true,
  totalDistanceKm: true,
  deliveryFeeHandled: true,
} as const;

export class ReportRepository {
  async findAdminDailyMetrics(from: Date, to: Date) {
    return prisma.reportAdminDailyMetric.findMany({
      where: {
        metricDate: {
          gte: from,
          lte: to,
        },
      },
      orderBy: {
        metricDate: "asc",
      },
      select: metricSelect,
    });
  }

  async findMerchantDailyMetrics(merchantId: string, from: Date, to: Date) {
    return prisma.reportMerchantDailyMetric.findMany({
      where: {
        merchantId,
        metricDate: {
          gte: from,
          lte: to,
        },
      },
      orderBy: {
        metricDate: "asc",
      },
      select: merchantMetricSelect,
    });
  }

  async findShipperDailyMetrics(shipperId: string, from: Date, to: Date) {
    return prisma.reportShipperDailyMetric.findMany({
      where: {
        shipperId,
        metricDate: {
          gte: from,
          lte: to,
        },
      },
      orderBy: {
        metricDate: "asc",
      },
      select: shipperMetricSelect,
    });
  }

  async findTopMerchants(from: Date, to: Date, limit: number) {
    return prisma.reportMerchantDailyMetric.findMany({
      where: {
        metricDate: {
          gte: from,
          lte: to,
        },
      },
      orderBy: [
        { netRevenue: "desc" },
        { orderCount: "desc" },
      ],
      take: limit,
      select: merchantMetricSelect,
    });
  }

  async findTopShippers(from: Date, to: Date, limit: number) {
    return prisma.reportShipperDailyMetric.findMany({
      where: {
        metricDate: {
          gte: from,
          lte: to,
        },
      },
      orderBy: [
        { deliveredOrderCount: "desc" },
        { completionRate: "desc" },
      ],
      take: limit,
      select: shipperMetricSelect,
    });
  }

  async findTopProducts(from: Date, to: Date, limit: number, merchantId?: string) {
    return prisma.reportOrderItemFact.findMany({
      where: {
        createdAt: {
          gte: from,
          lte: to,
        },
        ...(merchantId
          ? {
              order: {
                merchantId,
              },
            }
          : {}),
      },
      select: {
        productId: true,
        productName: true,
        productImage: true,
        quantity: true,
      },
    }).then((items) => {
      const grouped = new Map<string, { productId: string; productName: string; productImage: string | null; quantitySold: number; orderCount: number }>();

      for (const item of items) {
        const existing = grouped.get(item.productId);
        if (existing) {
          existing.quantitySold += item.quantity;
          existing.orderCount += 1;
        } else {
          grouped.set(item.productId, {
            productId: item.productId,
            productName: item.productName,
            productImage: item.productImage ?? null,
            quantitySold: item.quantity,
            orderCount: 1,
          });
        }
      }

      return Array.from(grouped.values())
        .sort((left, right) => right.quantitySold - left.quantitySold || right.orderCount - left.orderCount)
        .slice(0, limit);
    });
  }
}
