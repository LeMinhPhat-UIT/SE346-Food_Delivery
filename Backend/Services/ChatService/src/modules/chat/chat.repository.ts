import {
  ChatConversation,
  ChatConversationType,
  ChatMessage,
  ChatMessageStatus,
  ChatMessageType,
  ChatSenderRole,
  Prisma,
} from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";

export interface ConversationFilter {
  customerId?: string;
  merchantId?: string;
  shipperId?: string;
  conversationType?: ChatConversationType;
  page: number;
  limit: number;
}

export interface MessageFilter {
  conversationId: string;
  page: number;
  limit: number;
}

export interface CreateConversationInput {
  conversationType: ChatConversationType;
  orderId: string;
  deliveryId?: string;
  customerId: string;
  merchantId: string;
  shipperId?: string;
}

export interface CreateMessageInput {
  conversationId: string;
  senderRole: ChatSenderRole;
  senderId: string;
  content: string;
  messageType: ChatMessageType;
}

export class ChatRepository {
  async upsertConversation(input: CreateConversationInput): Promise<ChatConversation> {
    return prisma.chatConversation.upsert({
      where: {
        orderId_conversationType: {
          orderId: input.orderId,
          conversationType: input.conversationType,
        },
      },
      create: {
        conversationType: input.conversationType,
        orderId: input.orderId,
        deliveryId: input.deliveryId,
        customerId: input.customerId,
        merchantId: input.merchantId,
        shipperId: input.shipperId,
      },
      update: {
        deliveryId: input.deliveryId ?? undefined,
        customerId: input.customerId,
        merchantId: input.merchantId,
        shipperId: input.shipperId ?? undefined,
      },
    });
  }

  async findConversationById(conversationId: string): Promise<ChatConversation | null> {
    return prisma.chatConversation.findUnique({
      where: { id: conversationId },
    });
  }

  async findConversationByOrderAndType(
    orderId: string,
    conversationType: ChatConversationType,
  ): Promise<ChatConversation | null> {
    return prisma.chatConversation.findFirst({
      where: {
        orderId,
        conversationType,
      },
    });
  }

  async listConversations(filter: ConversationFilter) {
    const where: Prisma.ChatConversationWhereInput = {
      ...(filter.conversationType ? { conversationType: filter.conversationType } : {}),
      ...(filter.customerId ? { customerId: filter.customerId } : {}),
      ...(filter.merchantId ? { merchantId: filter.merchantId } : {}),
      ...(filter.shipperId ? { shipperId: filter.shipperId } : {}),
    };

    const skip = (filter.page - 1) * filter.limit;

    const total = await prisma.chatConversation.count({ where });
    const items = await prisma.chatConversation.findMany({
      where,
      orderBy: { updatedAt: "desc" },
      skip,
      take: filter.limit,
    });

    return { total, items };
  }

  async listMessages(filter: MessageFilter) {
    const skip = (filter.page - 1) * filter.limit;

    const total = await prisma.chatMessage.count({
      where: { conversationId: filter.conversationId },
    });
    const items = await prisma.chatMessage.findMany({
      where: { conversationId: filter.conversationId },
      orderBy: { createdAt: "asc" },
      skip,
      take: filter.limit,
    });

    return { total, items };
  }

  async createMessage(input: CreateMessageInput): Promise<ChatMessage> {
    return prisma.chatMessage.create({
      data: {
        conversationId: input.conversationId,
        senderRole: input.senderRole,
        senderId: input.senderId,
        content: input.content,
        messageType: input.messageType,
        status: ChatMessageStatus.SENT,
      },
    });
  }

  async updateConversationAfterMessage(
    conversationId: string,
    conversationType: ChatConversationType,
    senderRole: ChatSenderRole,
    preview: string,
  ): Promise<ChatConversation> {
    const updateData: Prisma.ChatConversationUpdateInput = {
      lastMessageAt: new Date(),
      lastMessagePreview: preview,
    };

    if (senderRole === ChatSenderRole.CUSTOMER) {
      if (conversationType === ChatConversationType.ORDER_MERCHANT) {
        updateData.unreadMerchantCount = { increment: 1 };
      }

      if (conversationType === ChatConversationType.ORDER_SHIPPER) {
        updateData.unreadShipperCount = { increment: 1 };
      }
    } else if (senderRole === ChatSenderRole.MERCHANT) {
      updateData.unreadCustomerCount = { increment: 1 };
    } else if (senderRole === ChatSenderRole.SHIPPER) {
      updateData.unreadCustomerCount = { increment: 1 };
    }

    return prisma.chatConversation.update({
      where: { id: conversationId },
      data: updateData,
    });
  }

  async markConversationAsRead(
    conversationId: string,
    actorRole: ChatSenderRole,
    actorId: string,
  ): Promise<ChatConversation> {
    const now = new Date();
    const updateData: Prisma.ChatConversationUpdateInput = {};

    if (actorRole === ChatSenderRole.CUSTOMER) {
      updateData.unreadCustomerCount = 0;
    } else if (actorRole === ChatSenderRole.MERCHANT) {
      updateData.unreadMerchantCount = 0;
    } else if (actorRole === ChatSenderRole.SHIPPER) {
      updateData.unreadShipperCount = 0;
    }

    await prisma.chatMessage.updateMany({
      where: {
        conversationId,
        senderId: { not: actorId },
        readAt: null,
      },
      data: {
        readAt: now,
        status: ChatMessageStatus.READ,
      },
    });

    return prisma.chatConversation.update({
      where: { id: conversationId },
      data: updateData,
    });
  }
}
