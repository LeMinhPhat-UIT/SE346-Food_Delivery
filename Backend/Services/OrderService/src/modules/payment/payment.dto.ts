import { PaymentMethod, PaymentTransactionStatus } from "@prisma/client";
import { z } from "zod";
import {
  createVnpayPaymentUrlBodySchema,
  orderIdParamSchema,
} from "./payment.schema";

export type OrderIdParamDto = z.infer<typeof orderIdParamSchema>;
export type CreateVnpayPaymentUrlDto = z.infer<
  typeof createVnpayPaymentUrlBodySchema
>;

export type PaymentDetailResponseDto = {
  id: string;
  orderId: string;
  orderNumber: string;
  method: PaymentMethod;
  status: PaymentTransactionStatus;
  amount: number;
  transactionId: string | null;
  paymentData: unknown;
  paidAt: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CreateVnpayPaymentUrlResponseDto = {
  orderId: string;
  orderNumber: string;
  amount: number;
  expiresAt: string;
  paymentUrl: string;
};

export type VnpayCallbackResponseDto = {
  rspCode: string;
  message: string;
  orderId?: string;
  orderNumber?: string;
  transactionId?: string | null;
  paymentStatus?: PaymentTransactionStatus;
  orderPaymentStatus?: string;
};

