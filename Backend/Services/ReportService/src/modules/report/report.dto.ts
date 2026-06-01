import { z } from "zod";
import { dateRangeQuerySchema } from "./report.schema";

export type DateRangeQueryDto = z.infer<typeof dateRangeQuerySchema>;

export type AdminDailyMetricDto = {
  metricDate: string;
  grossRevenue: number;
  netRevenue: number;
  platformRevenue: number;
  merchantCommissionTotal: number;
  shipperCommissionTotal: number;
  orderCount: number;
  paidOrderCount: number;
  cancelledOrderCount: number;
  deliveryFeeTotal: number;
  discountTotal: number;
  voucherUsageCount: number;
  codOrderCount: number;
  vnpayOrderCount: number;
  uniqueCustomers: number;
  uniqueMerchants: number;
  uniqueShippers: number;
};

export type MerchantDailyMetricDto = {
  metricDate: string;
  merchantId: string;
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
  avgOrderValue: number;
};

export type ShipperDailyMetricDto = {
  metricDate: string;
  shipperId: string;
  grossRevenue: number;
  netEarnings: number;
  shipperCommissionTotal: number;
  assignedOrderCount: number;
  pickedUpOrderCount: number;
  deliveredOrderCount: number;
  cancelledOrderCount: number;
  completionRate: number;
  avgDeliveryTimeMinutes: number;
  totalDistanceKm: number;
  deliveryFeeHandled: number;
};

export type TopMerchantDto = {
  merchantId: string;
  metricDate: string;
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
  avgOrderValue: number;
};

export type TopShipperDto = {
  metricDate: string;
  shipperId: string;
  grossRevenue: number;
  netEarnings: number;
  shipperCommissionTotal: number;
  assignedOrderCount: number;
  pickedUpOrderCount: number;
  deliveredOrderCount: number;
  cancelledOrderCount: number;
  completionRate: number;
  avgDeliveryTimeMinutes: number;
  totalDistanceKm: number;
  deliveryFeeHandled: number;
};

export type TopProductDto = {
  productId: string;
  productName: string;
  productImage: string | null;
  quantitySold: number;
  orderCount: number;
};

export type ReportSummaryDto = Record<string, number>;

export type ReportOverviewResponseDto<TDaily> = {
  from: string;
  to: string;
  summary: ReportSummaryDto;
  daily: TDaily[];
};
