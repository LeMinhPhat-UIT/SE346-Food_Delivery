import { ReportRepository } from "./report.repository";
import { env } from "../../config/env.config";
import {
  AdminDailyMetricDto,
  MerchantDailyMetricDto,
  ReportOverviewResponseDto,
  TopMerchantDto,
  TopProductDto,
  TopShipperDto,
  ShipperDailyMetricDto,
} from "./report.dto";
import { toNumber } from "./report.mapper";
import { ApiError } from "../../utils/apiError";
import { HTTP_STATUS } from "../../constants/httpStatus";

type DateRangeInput = {
  from?: string;
  to?: string;
};

export class ReportService {
  constructor(private readonly reportRepository: ReportRepository) {}

  async getAdminOverview(query: DateRangeInput): Promise<ReportOverviewResponseDto<AdminDailyMetricDto>> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findAdminDailyMetrics(from, to);
    const uniqueCounts = await this.reportRepository.findAdminUniqueCounts(from, to);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      summary: this.buildAdminSummary(rows, uniqueCounts),
      daily: rows.map((row) => ({
        metricDate: row.metricDate.toISOString(),
        grossRevenue: toNumber(row.grossRevenue),
        netRevenue: toNumber(row.netRevenue),
        platformRevenue: toNumber(row.platformRevenue),
        merchantCommissionTotal: toNumber(row.merchantCommissionTotal),
        shipperCommissionTotal: toNumber(row.shipperCommissionTotal),
        orderCount: row.orderCount,
        paidOrderCount: row.paidOrderCount,
        cancelledOrderCount: row.cancelledOrderCount,
        deliveryFeeTotal: toNumber(row.deliveryFeeTotal),
        discountTotal: toNumber(row.discountTotal),
        voucherUsageCount: row.voucherUsageCount,
        codOrderCount: row.codOrderCount,
        vnpayOrderCount: row.vnpayOrderCount,
        uniqueCustomers: row.uniqueCustomers,
        uniqueMerchants: row.uniqueMerchants,
        uniqueShippers: row.uniqueShippers,
      })),
    };
  }

  async getMerchantOverview(merchantId: string, query: DateRangeInput): Promise<ReportOverviewResponseDto<MerchantDailyMetricDto>> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findMerchantDailyMetrics(merchantId, from, to);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      summary: this.buildMerchantSummary(rows),
      daily: rows.map((row) => ({
        metricDate: row.metricDate.toISOString(),
        merchantId: row.merchantId,
        grossRevenue: toNumber(row.grossRevenue),
        netRevenue: toNumber(row.netRevenue),
        merchantCommissionTotal: toNumber(row.merchantCommissionTotal),
        orderCount: row.orderCount,
        paidOrderCount: row.paidOrderCount,
        cancelledOrderCount: row.cancelledOrderCount,
        subtotalRevenue: toNumber(row.subtotalRevenue),
        deliveryFeeRevenue: toNumber(row.deliveryFeeRevenue),
        discountTotal: toNumber(row.discountTotal),
        voucherUsageCount: row.voucherUsageCount,
        avgOrderValue: toNumber(row.avgOrderValue),
      })),
    };
  }

  async getShipperOverview(shipperId: string, query: DateRangeInput): Promise<ReportOverviewResponseDto<ShipperDailyMetricDto>> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findShipperDailyMetrics(shipperId, from, to);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      summary: this.buildShipperSummary(rows),
      daily: rows.map((row) => ({
        metricDate: row.metricDate.toISOString(),
        shipperId: row.shipperId,
        grossRevenue: toNumber(row.grossRevenue),
        netEarnings: toNumber(row.netEarnings),
        shipperCommissionTotal: toNumber(row.shipperCommissionTotal),
        assignedOrderCount: row.assignedOrderCount,
        pickedUpOrderCount: row.pickedUpOrderCount,
        deliveredOrderCount: row.deliveredOrderCount,
        cancelledOrderCount: row.cancelledOrderCount,
        completionRate: toNumber(row.completionRate),
        avgDeliveryTimeMinutes: toNumber(row.avgDeliveryTimeMinutes),
        totalDistanceKm: toNumber(row.totalDistanceKm),
        deliveryFeeHandled: toNumber(row.deliveryFeeHandled),
      })),
    };
  }

  async getTopMerchants(query: DateRangeInput): Promise<{ from: string; to: string; items: TopMerchantDto[] }> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findTopMerchants(from, to);
    const items = this.groupTopMerchants(rows).slice(0, 10);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      items,
    };
  }

  async getTopShippers(query: DateRangeInput): Promise<{ from: string; to: string; items: TopShipperDto[] }> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findTopShippers(from, to);
    const items = this.groupTopShippers(rows).slice(0, 10);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      items,
    };
  }

  async getTopProducts(
    query: DateRangeInput,
    merchantId?: string,
  ): Promise<{ from: string; to: string; items: TopProductDto[] }> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findTopProducts(from, to, 10, merchantId);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      items: rows.map((row) => ({
        productId: row.productId,
        productName: row.productName,
        productImage: row.productImage ?? null,
        quantitySold: row.quantitySold,
        orderCount: row.orderCount,
      })),
    };
  }

  private resolveDateRange(query: DateRangeInput) {
    const to = query.to ? this.parseDateOrThrow(query.to, "to") : new Date();
    const from = query.from
      ? this.parseDateOrThrow(query.from, "from")
      : new Date(to.getTime() - 29 * 24 * 60 * 60 * 1000);

    return { from, to };
  }

  private parseDateOrThrow(value: string, field: string) {
    const parsed = new Date(value);

    if (Number.isNaN(parsed.getTime())) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, `Invalid ${field} date`);
    }

    return parsed;
  }

  private buildAdminSummary(
    rows: Awaited<ReturnType<ReportRepository["findAdminDailyMetrics"]>>,
    uniqueCounts: Awaited<ReturnType<ReportRepository["findAdminUniqueCounts"]>>,
  ) {
    return rows.reduce(
      (summary, row) => ({
        grossRevenue: summary.grossRevenue + toNumber(row.grossRevenue),
        netRevenue: summary.netRevenue + toNumber(row.netRevenue),
        platformRevenue: summary.platformRevenue + toNumber(row.platformRevenue),
        merchantCommissionTotal:
          summary.merchantCommissionTotal + toNumber(row.merchantCommissionTotal),
        shipperCommissionTotal:
          summary.shipperCommissionTotal + toNumber(row.shipperCommissionTotal),
        orderCount: summary.orderCount + row.orderCount,
        paidOrderCount: summary.paidOrderCount + row.paidOrderCount,
        cancelledOrderCount: summary.cancelledOrderCount + row.cancelledOrderCount,
        deliveryFeeTotal: summary.deliveryFeeTotal + toNumber(row.deliveryFeeTotal),
        discountTotal: summary.discountTotal + toNumber(row.discountTotal),
        voucherUsageCount: summary.voucherUsageCount + row.voucherUsageCount,
        codOrderCount: summary.codOrderCount + row.codOrderCount,
        vnpayOrderCount: summary.vnpayOrderCount + row.vnpayOrderCount,
        uniqueCustomers: uniqueCounts.uniqueCustomers,
        uniqueMerchants: uniqueCounts.uniqueMerchants,
        uniqueShippers: uniqueCounts.uniqueShippers,
      }),
      {
        grossRevenue: 0,
        netRevenue: 0,
        platformRevenue: 0,
        merchantCommissionTotal: 0,
        shipperCommissionTotal: 0,
        orderCount: 0,
        paidOrderCount: 0,
        cancelledOrderCount: 0,
        deliveryFeeTotal: 0,
        discountTotal: 0,
        voucherUsageCount: 0,
        codOrderCount: 0,
        vnpayOrderCount: 0,
        uniqueCustomers: uniqueCounts.uniqueCustomers,
        uniqueMerchants: uniqueCounts.uniqueMerchants,
        uniqueShippers: uniqueCounts.uniqueShippers,
      },
    );
  }

  private buildMerchantSummary(rows: Awaited<ReturnType<ReportRepository["findMerchantDailyMetrics"]>>) {
    const summary = rows.reduce(
      (acc, row) => ({
        grossRevenue: acc.grossRevenue + toNumber(row.grossRevenue),
        netRevenue: acc.netRevenue + toNumber(row.netRevenue),
        merchantCommissionTotal:
          acc.merchantCommissionTotal + toNumber(row.merchantCommissionTotal),
        orderCount: acc.orderCount + row.orderCount,
        paidOrderCount: acc.paidOrderCount + row.paidOrderCount,
        cancelledOrderCount: acc.cancelledOrderCount + row.cancelledOrderCount,
        subtotalRevenue: acc.subtotalRevenue + toNumber(row.subtotalRevenue),
        deliveryFeeRevenue: acc.deliveryFeeRevenue + toNumber(row.deliveryFeeRevenue),
        discountTotal: acc.discountTotal + toNumber(row.discountTotal),
        voucherUsageCount: acc.voucherUsageCount + row.voucherUsageCount,
      }),
      {
        grossRevenue: 0,
        netRevenue: 0,
        merchantCommissionTotal: 0,
        orderCount: 0,
        paidOrderCount: 0,
        cancelledOrderCount: 0,
        subtotalRevenue: 0,
        deliveryFeeRevenue: 0,
        discountTotal: 0,
        voucherUsageCount: 0,
      },
    );

    return {
      ...summary,
      avgOrderValue: summary.orderCount ? summary.netRevenue / summary.orderCount : 0,
    };
  }

  private buildShipperSummary(rows: Awaited<ReturnType<ReportRepository["findShipperDailyMetrics"]>>) {
    const weightedCompletionDenominator = rows.reduce(
      (acc, row) => acc + row.assignedOrderCount,
      0,
    );
    const weightedDeliveryTimeDenominator = rows.reduce(
      (acc, row) => acc + row.deliveredOrderCount,
      0,
    );

    const summary = rows.reduce(
      (acc, row) => ({
        grossRevenue: acc.grossRevenue + toNumber(row.grossRevenue),
        netEarnings: acc.netEarnings + toNumber(row.netEarnings),
        shipperCommissionTotal:
          acc.shipperCommissionTotal + toNumber(row.shipperCommissionTotal),
        assignedOrderCount: acc.assignedOrderCount + row.assignedOrderCount,
        pickedUpOrderCount: acc.pickedUpOrderCount + row.pickedUpOrderCount,
        deliveredOrderCount: acc.deliveredOrderCount + row.deliveredOrderCount,
        cancelledOrderCount: acc.cancelledOrderCount + row.cancelledOrderCount,
        completionRate: acc.completionRate + toNumber(row.completionRate),
        avgDeliveryTimeMinutes: acc.avgDeliveryTimeMinutes + toNumber(row.avgDeliveryTimeMinutes),
        totalDistanceKm: acc.totalDistanceKm + toNumber(row.totalDistanceKm),
        deliveryFeeHandled: acc.deliveryFeeHandled + toNumber(row.deliveryFeeHandled),
      }),
      {
        grossRevenue: 0,
        netEarnings: 0,
        shipperCommissionTotal: 0,
        assignedOrderCount: 0,
        pickedUpOrderCount: 0,
        deliveredOrderCount: 0,
        cancelledOrderCount: 0,
        completionRate: 0,
        avgDeliveryTimeMinutes: 0,
        totalDistanceKm: 0,
        deliveryFeeHandled: 0,
      },
    );

    return {
      ...summary,
      completionRate: weightedCompletionDenominator
        ? this.roundMoney(
            (summary.deliveredOrderCount / weightedCompletionDenominator) * 100,
          )
        : 0,
      avgDeliveryTimeMinutes: weightedDeliveryTimeDenominator
        ? this.roundMoney(
            summary.avgDeliveryTimeMinutes / weightedDeliveryTimeDenominator,
          )
        : 0,
    };
  }

  private groupTopMerchants(rows: Awaited<ReturnType<ReportRepository["findTopMerchants"]>>) {
    const grouped = new Map<
      string,
      {
        merchantId: string;
        metricDate: Date;
        grossRevenue: number;
        netRevenue: number;
        merchantCommissionTotal: number;
        orderCount: number;
        paidOrderCount: number;
        cancelledOrderCount: number;
        subtotalRevenue: number;
        deliveryFeeRevenue: number;
        discountTotal: number;
        voucherUsageCount: number;
      }
    >();

    for (const row of rows) {
      const existing = grouped.get(row.merchantId);
      const metricDate = row.metricDate;

      if (!existing) {
        grouped.set(row.merchantId, {
          merchantId: row.merchantId,
          metricDate,
          grossRevenue: toNumber(row.grossRevenue),
          netRevenue: toNumber(row.netRevenue),
          merchantCommissionTotal: toNumber(row.merchantCommissionTotal),
          orderCount: row.orderCount,
          paidOrderCount: row.paidOrderCount,
          cancelledOrderCount: row.cancelledOrderCount,
          subtotalRevenue: toNumber(row.subtotalRevenue),
          deliveryFeeRevenue: toNumber(row.deliveryFeeRevenue),
          discountTotal: toNumber(row.discountTotal),
          voucherUsageCount: row.voucherUsageCount,
        });
        continue;
      }

      existing.metricDate = existing.metricDate > metricDate ? existing.metricDate : metricDate;
      existing.grossRevenue = this.roundMoney(existing.grossRevenue + toNumber(row.grossRevenue));
      existing.netRevenue = this.roundMoney(existing.netRevenue + toNumber(row.netRevenue));
      existing.merchantCommissionTotal = this.roundMoney(
        existing.merchantCommissionTotal + toNumber(row.merchantCommissionTotal),
      );
      existing.orderCount += row.orderCount;
      existing.paidOrderCount += row.paidOrderCount;
      existing.cancelledOrderCount += row.cancelledOrderCount;
      existing.subtotalRevenue = this.roundMoney(
        existing.subtotalRevenue + toNumber(row.subtotalRevenue),
      );
      existing.deliveryFeeRevenue = this.roundMoney(
        existing.deliveryFeeRevenue + toNumber(row.deliveryFeeRevenue),
      );
      existing.discountTotal = this.roundMoney(existing.discountTotal + toNumber(row.discountTotal));
      existing.voucherUsageCount += row.voucherUsageCount;
    }

    return Array.from(grouped.values())
      .sort((left, right) => right.netRevenue - left.netRevenue || right.orderCount - left.orderCount)
      .map((row) => ({
        metricDate: row.metricDate.toISOString(),
        merchantId: row.merchantId,
        grossRevenue: this.roundMoney(row.grossRevenue),
        netRevenue: this.roundMoney(row.netRevenue),
        merchantCommissionTotal: this.roundMoney(row.merchantCommissionTotal),
        orderCount: row.orderCount,
        paidOrderCount: row.paidOrderCount,
        cancelledOrderCount: row.cancelledOrderCount,
        subtotalRevenue: this.roundMoney(row.subtotalRevenue),
        deliveryFeeRevenue: this.roundMoney(row.deliveryFeeRevenue),
        discountTotal: this.roundMoney(row.discountTotal),
        voucherUsageCount: row.voucherUsageCount,
        avgOrderValue: row.orderCount ? this.roundMoney(row.netRevenue / row.orderCount) : 0,
      }));
  }

  private groupTopShippers(rows: Awaited<ReturnType<ReportRepository["findTopShippers"]>>) {
    const grouped = new Map<
      string,
      {
        shipperId: string;
        metricDate: Date;
        grossRevenue: number;
        netEarnings: number;
        shipperCommissionTotal: number;
        assignedOrderCount: number;
        pickedUpOrderCount: number;
        deliveredOrderCount: number;
        cancelledOrderCount: number;
        totalDistanceKm: number;
        deliveryFeeHandled: number;
        weightedDeliveryTimeMinutes: number;
      }
    >();

    for (const row of rows) {
      const existing = grouped.get(row.shipperId);
      const metricDate = row.metricDate;
      const deliveryTimeWeight = row.deliveredOrderCount || 0;

      if (!existing) {
        grouped.set(row.shipperId, {
          shipperId: row.shipperId,
          metricDate,
          grossRevenue: toNumber(row.grossRevenue),
          netEarnings: toNumber(row.netEarnings),
          shipperCommissionTotal: toNumber(row.shipperCommissionTotal),
          assignedOrderCount: row.assignedOrderCount,
          pickedUpOrderCount: row.pickedUpOrderCount,
          deliveredOrderCount: row.deliveredOrderCount,
          cancelledOrderCount: row.cancelledOrderCount,
          totalDistanceKm: toNumber(row.totalDistanceKm),
          deliveryFeeHandled: toNumber(row.deliveryFeeHandled),
          weightedDeliveryTimeMinutes: toNumber(row.avgDeliveryTimeMinutes) * deliveryTimeWeight,
        });
        continue;
      }

      existing.metricDate = existing.metricDate > metricDate ? existing.metricDate : metricDate;
      existing.grossRevenue = this.roundMoney(existing.grossRevenue + toNumber(row.grossRevenue));
      existing.netEarnings = this.roundMoney(existing.netEarnings + toNumber(row.netEarnings));
      existing.shipperCommissionTotal = this.roundMoney(
        existing.shipperCommissionTotal + toNumber(row.shipperCommissionTotal),
      );
      existing.assignedOrderCount += row.assignedOrderCount;
      existing.pickedUpOrderCount += row.pickedUpOrderCount;
      existing.deliveredOrderCount += row.deliveredOrderCount;
      existing.cancelledOrderCount += row.cancelledOrderCount;
      existing.totalDistanceKm = this.roundMoney(existing.totalDistanceKm + toNumber(row.totalDistanceKm));
      existing.deliveryFeeHandled = this.roundMoney(
        existing.deliveryFeeHandled + toNumber(row.deliveryFeeHandled),
      );
      existing.weightedDeliveryTimeMinutes += toNumber(row.avgDeliveryTimeMinutes) * deliveryTimeWeight;
    }

    return Array.from(grouped.values())
      .sort((left, right) => right.deliveredOrderCount - left.deliveredOrderCount || right.assignedOrderCount - left.assignedOrderCount)
      .map((row) => ({
        metricDate: row.metricDate.toISOString(),
        shipperId: row.shipperId,
        grossRevenue: this.roundMoney(row.grossRevenue),
        netEarnings: this.roundMoney(row.netEarnings),
        shipperCommissionTotal: this.roundMoney(row.shipperCommissionTotal),
        assignedOrderCount: row.assignedOrderCount,
        pickedUpOrderCount: row.pickedUpOrderCount,
        deliveredOrderCount: row.deliveredOrderCount,
        cancelledOrderCount: row.cancelledOrderCount,
        completionRate: row.assignedOrderCount
          ? this.roundMoney((row.deliveredOrderCount / row.assignedOrderCount) * 100)
          : 0,
        avgDeliveryTimeMinutes: row.deliveredOrderCount
          ? this.roundMoney(row.weightedDeliveryTimeMinutes / row.deliveredOrderCount)
          : 0,
        totalDistanceKm: this.roundMoney(row.totalDistanceKm),
        deliveryFeeHandled: this.roundMoney(row.deliveryFeeHandled),
      }));
  }

  private roundMoney(value: number) {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
