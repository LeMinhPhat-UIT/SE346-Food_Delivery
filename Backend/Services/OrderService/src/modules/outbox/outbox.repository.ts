import { OutboxStatus } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";

export type OutboxMessageRecord = {
  id: string;
  aggregateType: string;
  aggregateId: string;
  eventType: string;
  payload: unknown;
  status: OutboxStatus;
  createdAt: Date;
};

export class OutboxRepository {
  async findPending(limit: number): Promise<OutboxMessageRecord[]> {
    return prisma.outboxMessage.findMany({
      where: {
        status: OutboxStatus.PENDING,
      },
      orderBy: {
        createdAt: "asc",
      },
      take: limit,
    });
  }

  async markPublished(id: string) {
    await prisma.outboxMessage.update({
      where: { id },
      data: {
        status: OutboxStatus.PUBLISHED,
      },
    });
  }
}
