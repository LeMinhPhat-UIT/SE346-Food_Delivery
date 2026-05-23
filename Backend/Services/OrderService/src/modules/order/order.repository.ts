import { prisma } from "../../prisma/prisma.client";
import { PaymentMethod, Prisma } from "@prisma/client";
import { MyOrdersQueryDto } from "./order.dto";

const orderListSelect = {
  id: true,
  orderNumber: true,
  merchantId: true,
  merchantName: true,
  merchantAvatar: true,
  subtotal: true,
  deliveryFee: true,
  discountAmount: true,
  totalAmount: true,
  paymentMethod: true,
  paymentStatus: true,
  status: true,
  createdAt: true,
  items: {
    select: {
      id: true,
      productId: true,
      productName: true,
      productImage: true,
      quantity: true,
    },
    orderBy: {
      createdAt: "asc" as const,
    },
  },
} as const;

const orderDetailSelect = {
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
  cancelReason: true,
  cancelledBy: true,
  note: true,
  voucherId: true,
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
  statusHistory: {
    select: {
      id: true,
      status: true,
      note: true,
      createdBy: true,
      createdAt: true,
    },
    orderBy: {
      createdAt: "asc" as const,
    },
  },
} as const;

export class OrderRepository {
  async isVoucherUsedByOrder(voucherId: string, userId: string) {
    return prisma.voucherUsage.count({
      where: {
        voucherId,
        userId,
      },
    });
  }

  async createOrder(payload: {
    orderNumber: string;
    userId: string;
    merchantId: string;
    merchantName: string;
    merchantAvatar: string | null;
    deliveryAddress: string;
    deliveryWard: string | null;
    deliveryDistrict: string | null;
    deliveryCity: string | null;
    deliveryLat: number;
    deliveryLng: number;
    recipientName: string;
    recipientPhone: string;
    subtotal: number;
    deliveryFee: number;
    discountAmount: number;
    totalAmount: number;
    paymentMethod: PaymentMethod;
    note: string | null;
    voucherId: string | null;
      items: Array<{
        productId: string;
        productName: string;
        productImage: string | null;
        unitPrice: number;
        selectedOptions: Prisma.InputJsonValue;
        quantity: number;
        note: string | null;
      }>;
    voucherUsage?: {
      voucherId: string;
      userId: string;
      discountAmount: number;
    } | null;
    orderCompletedEvent?: {
      orderNumber: string;
      merchantId: string;
      merchantName: string;
      merchantAddress: {
        addressLine: string;
        lat: number;
        lng: number;
      };
      userId: string;
      customerName: string;
      customerPhone: string;
      deliveryAddress: {
        addressLine: string;
        lat: number;
        lng: number;
      };
      totalAmount: number;
      paymentMethod: PaymentMethod;
      note: string | null;
    };
  }) {
    return prisma.$transaction(async (tx) => {
      const order = await tx.order.create({
        data: {
          orderNumber: payload.orderNumber,
          userId: payload.userId,
          merchantId: payload.merchantId,
          merchantName: payload.merchantName,
          merchantAvatar: payload.merchantAvatar,
          deliveryAddress: payload.deliveryAddress,
          deliveryWard: payload.deliveryWard,
          deliveryDistrict: payload.deliveryDistrict,
          deliveryCity: payload.deliveryCity,
          deliveryLat: payload.deliveryLat,
          deliveryLng: payload.deliveryLng,
          recipientName: payload.recipientName,
          recipientPhone: payload.recipientPhone,
          subtotal: payload.subtotal,
          deliveryFee: payload.deliveryFee,
          discountAmount: payload.discountAmount,
          totalAmount: payload.totalAmount,
          paymentMethod: payload.paymentMethod,
          note: payload.note,
          voucherId: payload.voucherId,
          items: {
            create: payload.items,
          },
          statusHistory: {
            create: {
              status: "PENDING",
              createdBy: payload.userId,
              note: "Order created",
            },
          },
        },
      });

      if (payload.voucherUsage) {
        await tx.voucherUsage.create({
          data: {
            voucherId: payload.voucherUsage.voucherId,
            userId: payload.voucherUsage.userId,
            orderId: order.id,
            discountAmount: payload.voucherUsage.discountAmount,
          },
        });
      }

      if (payload.orderCompletedEvent) {
        await tx.outboxMessage.create({
          data: {
            aggregateType: "Order",
            aggregateId: order.id,
            eventType: "order.completed",
            payload: {
              OrderId: order.id,
              OrderNumber: payload.orderCompletedEvent.orderNumber,
              OrderStatus: order.status,
              MerchantId: payload.orderCompletedEvent.merchantId,
              MerchantStoreName: payload.orderCompletedEvent.merchantName,
              MerchantAddress: {
                AddressLine: payload.orderCompletedEvent.merchantAddress.addressLine,
                Lat: payload.orderCompletedEvent.merchantAddress.lat,
                Lng: payload.orderCompletedEvent.merchantAddress.lng,
              },
              UserId: payload.orderCompletedEvent.userId,
              CustomerName: payload.orderCompletedEvent.customerName,
              CustomerPhone: payload.orderCompletedEvent.customerPhone,
              DeliveryAddress: {
                AddressLine: payload.orderCompletedEvent.deliveryAddress.addressLine,
                Lat: payload.orderCompletedEvent.deliveryAddress.lat,
                Lng: payload.orderCompletedEvent.deliveryAddress.lng,
              },
              TotalAmount: payload.orderCompletedEvent.totalAmount,
              PaymentMethod: payload.orderCompletedEvent.paymentMethod,
              Note: payload.orderCompletedEvent.note,
            } satisfies Prisma.InputJsonValue,
          },
        });
      }

      return order;
    });
  }

  async findMyOrders(userId: string, query: MyOrdersQueryDto) {
    const where: Prisma.OrderWhereInput = {
      userId,
      merchantId: query.merchantId,
      status: query.status,
      paymentStatus: query.paymentStatus,
    };

    const [items, totalCount] = await Promise.all([
      prisma.order.findMany({
        where,
        orderBy: {
          [query.sortBy]: query.sortOrder,
        },
        skip: (query.page - 1) * query.limit,
        take: query.limit,
        select: orderListSelect,
      }),
      prisma.order.count({ where }),
    ]);

    return { items, totalCount };
  }

  async findByIdForUser(id: string, userId: string) {
    return prisma.order.findFirst({
      where: {
        id,
        userId,
      },
      select: orderDetailSelect,
    });
  }

  async findById(id: string) {
    return prisma.order.findFirst({
      where: {
        id,
      },
      select: orderDetailSelect,
    });
  }

  async findMerchantOrders(merchantId: string, query: MyOrdersQueryDto) {
    const where: Prisma.OrderWhereInput = {
      merchantId,
      status: query.status,
      paymentStatus: query.paymentStatus,
    };

    const [items, totalCount] = await Promise.all([
      prisma.order.findMany({
        where,
        orderBy: {
          [query.sortBy]: query.sortOrder,
        },
        skip: (query.page - 1) * query.limit,
        take: query.limit,
        select: orderListSelect,
      }),
      prisma.order.count({ where }),
    ]);

    return { items, totalCount };
  }

  async findByIdForMerchant(id: string, merchantId: string) {
    return prisma.order.findFirst({
      where: {
        id,
        merchantId,
      },
      select: orderDetailSelect,
    });
  }

  async updateOrderStatus(payload: {
    orderId: string;
    status:
      | "PENDING"
      | "CONFIRMED"
      | "PREPARING"
      | "READY"
      | "PICKED_UP"
      | "DELIVERING"
      | "DELIVERED"
      | "CANCELLED";
    note?: string | null;
    cancelReason?: string | null;
    cancelledBy?: "CUSTOMER" | "MERCHANT" | "SHIPPER" | "SYSTEM" | null;
    createdBy: string;
  }) {
    return prisma.$transaction(async (tx) => {
      const order = await tx.order.update({
        where: { id: payload.orderId },
        data: {
          status: payload.status,
          cancelReason: payload.status === "CANCELLED" ? payload.cancelReason ?? null : null,
          cancelledBy: payload.status === "CANCELLED" ? payload.cancelledBy ?? null : null,
          statusHistory: {
            create: {
              status: payload.status,
              note:
                payload.note ??
                (payload.status === "CANCELLED"
                  ? payload.cancelReason ?? "Order cancelled"
                  : `Order updated to ${payload.status}`),
              createdBy: payload.createdBy,
            },
          },
        },
        select: orderDetailSelect,
      });

      return order;
    });
  }
}
