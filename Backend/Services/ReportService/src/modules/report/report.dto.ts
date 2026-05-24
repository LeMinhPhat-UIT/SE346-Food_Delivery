import { z } from "zod";
import { dateRangeQuerySchema } from "./report.schema";

export type DateRangeQueryDto = z.infer<typeof dateRangeQuerySchema>;

export type AdminDailyMetricDto = {
  metricDate: string;
  grossRevenue: number;
  netRevenue: number;
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
  assignedOrderCount: number;
  pickedUpOrderCount: number;
  deliveredOrderCount: number;
  cancelledOrderCount: number;
  completionRate: number;
  avgDeliveryTimeMinutes: number;
  totalDistanceKm: number;
  deliveryFeeHandled: number;
};

export type ReportOverviewResponseDto<TDaily> = {
  from: string;
  to: string;
  summary: Record<string, number>;
  daily: TDaily[];
};
