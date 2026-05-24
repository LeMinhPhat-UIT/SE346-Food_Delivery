import { Payment, Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";

const paymentWithOrderSelect = {
  id: true,
  orderId: true,
  idempotencyKey: true,
  method: true,
  status: true,
  amount: true,
  transactionId: true,
  paymentData: true,
  paidAt: true,
  createdAt: true,
  updatedAt: true,
  order: {
    select: {
      id: true,
      orderNumber: true,
      userId: true,
      merchantId: true,
      merchantName: true,
      merchantAvatar: true,
      deliveryAddress: true,
      deliveryWard: true,
      deliveryDistrict: true,
      deliveryCity: true,
      deliveryLat: true,
      deliveryLng: true,
      recipientName: true,
      recipientPhone: true,
      subtotal: true,
      deliveryFee: true,
      discountAmount: true,
      totalAmount: true,
      paymentMethod: true,
      paymentStatus: true,
      status: true,
      note: true,
      createdAt: true,
      updatedAt: true,
      items: {
        select: {
          id: true,
          productId: true,
          productName: true,
          productImage: true,
          unitPrice: true,
          selectedOptions: true,
          quantity: true,
          note: true,
          createdAt: true,
        },
        orderBy: {
          createdAt: "asc" as const,
        },
      },
    },
  },
} as const;

export type PaymentWithOrder = Prisma.PaymentGetPayload<{
  select: typeof paymentWithOrderSelect;
}>;

export class PaymentRepository {
  async findByOrderId(orderId: string): Promise<PaymentWithOrder | null> {
    return prisma.payment.findFirst({
      where: { orderId },
      select: paymentWithOrderSelect,
    });
  }

  async findByTxnRef(txnRef: string): Promise<PaymentWithOrder | null> {
    return prisma.payment.findFirst({
      where: {
        idempotencyKey: txnRef,
      },
      select: paymentWithOrderSelect,
    });
  }

  async updateById(
    id: string,
    data: Prisma.PaymentUpdateInput,
  ): Promise<PaymentWithOrder> {
    return prisma.payment.update({
      where: { id },
      data,
      select: paymentWithOrderSelect,
    });
  }
}

