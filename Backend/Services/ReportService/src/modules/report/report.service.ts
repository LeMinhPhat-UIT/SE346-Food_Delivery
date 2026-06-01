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

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      summary: this.buildAdminSummary(rows),
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
    const rows = await this.reportRepository.findTopMerchants(from, to, 10);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      items: rows.map((row) => ({
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

  async getTopShippers(query: DateRangeInput): Promise<{ from: string; to: string; items: TopShipperDto[] }> {
    const { from, to } = this.resolveDateRange(query);
    const rows = await this.reportRepository.findTopShippers(from, to, 10);

    return {
      from: from.toISOString(),
      to: to.toISOString(),
      items: rows.map((row) => ({
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

  private buildAdminSummary(rows: Awaited<ReturnType<ReportRepository["findAdminDailyMetrics"]>>) {
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
        uniqueCustomers: summary.uniqueCustomers + row.uniqueCustomers,
        uniqueMerchants: summary.uniqueMerchants + row.uniqueMerchants,
        uniqueShippers: summary.uniqueShippers + row.uniqueShippers,
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
        uniqueCustomers: 0,
        uniqueMerchants: 0,
        uniqueShippers: 0,
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
      completionRate: rows.length ? summary.completionRate / rows.length : 0,
      avgDeliveryTimeMinutes: rows.length ? summary.avgDeliveryTimeMinutes / rows.length : 0,
    };
  }
}
