import { prisma } from "../../prisma/prisma.client";

export class OrderRepository {
  async isVoucherUsedByOrder(voucherId: string, userId: string) {
    return prisma.voucherUsage.count({
      where: {
        voucherId,
        userId,
      },
    });
  }
}
