import { Payment, PaymentMethod, PaymentTransactionStatus } from "@prisma/client";
import { PaymentDetailResponseDto } from "./payment.dto";

export const toPaymentDetailResponseDto = (
  payment: Payment & {
    order?: {
      orderNumber: string;
    } | null;
  },
): PaymentDetailResponseDto => {
  return {
    id: payment.id,
    orderId: payment.orderId,
    orderNumber: payment.order?.orderNumber ?? payment.idempotencyKey,
    method: payment.method,
    status: payment.status,
    amount: Number(payment.amount),
    transactionId: payment.transactionId,
    paymentData: payment.paymentData ?? null,
    paidAt: payment.paidAt ? payment.paidAt.toISOString() : null,
    createdAt: payment.createdAt.toISOString(),
    updatedAt: payment.updatedAt.toISOString(),
  };
};

export const isSuccessfulVnpayResult = (
  responseCode?: string,
  transactionStatus?: string,
) => responseCode === "00" && transactionStatus === "00";

