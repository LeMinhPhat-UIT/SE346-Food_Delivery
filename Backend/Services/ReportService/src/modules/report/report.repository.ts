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
}
