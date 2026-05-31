import { ChatConversation, ChatConversationType, ChatMessageType, ChatSenderRole } from "@prisma/client";
import { ApiError } from "../../utils/apiError";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ROLES } from "../../constants/roles";
import {
  ChatConversationDto,
  ChatMessageDto,
  PaginatedChatResponse,
} from "./chat.dto";
import { ChatRepository } from "./chat.repository";
import { toConversationDto, toMessageDto } from "./chat.mapper";

import { Request } from "express";

type AuthContext = NonNullable<Request["auth"]>;

export interface CreateConversationCommand {
  conversationType: ChatConversationType;
  orderId: string;
  deliveryId?: string;
  customerId: string;
  merchantId: string;
  shipperId?: string;
}

export interface CreateMessageCommand {
  content: string;
  messageType: ChatMessageType;
}

export interface ConversationQuery {
  page: number;
  limit: number;
  conversationType?: ChatConversationType;
}

export interface MessageQuery {
  page: number;
  limit: number;
}

export class ChatService {
  constructor(private readonly chatRepository = new ChatRepository()) {}

  async createConversation(auth: AuthContext, command: CreateConversationCommand): Promise<ChatConversationDto> {
    this.assertConversationOwnership(auth, command);

    const conversation = await this.chatRepository.upsertConversation(command);
    return toConversationDto(conversation);
  }

  async getConversation(auth: AuthContext, conversationId: string): Promise<ChatConversationDto> {
    const conversation = await this.findAccessibleConversation(auth, conversationId);
    return toConversationDto(conversation);
  }

  async listConversations(
    auth: AuthContext,
    query: ConversationQuery,
  ): Promise<PaginatedChatResponse<ChatConversationDto>> {
    const filter = this.buildConversationFilter(auth, query);
    const { total, items } = await this.chatRepository.listConversations(filter);

    return {
      items: items.map(toConversationDto),
      pagination: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async getMessages(
    auth: AuthContext,
    conversationId: string,
    query: MessageQuery,
  ): Promise<PaginatedChatResponse<ChatMessageDto>> {
    await this.findAccessibleConversation(auth, conversationId);

    const { total, items } = await this.chatRepository.listMessages({
      conversationId,
      page: query.page,
      limit: query.limit,
    });

    return {
      items: items.map(toMessageDto),
      pagination: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async sendMessage(
    auth: AuthContext,
    conversationId: string,
    command: CreateMessageCommand,
  ): Promise<ChatMessageDto> {
    const conversation = await this.findAccessibleConversation(auth, conversationId);
    const actor = this.resolveActor(auth);
    const senderId = this.resolveSenderId(auth, actor);

    const message = await this.chatRepository.createMessage({
      conversationId,
      senderRole: actor,
      senderId,
      content: command.content,
      messageType: command.messageType,
    });

    await this.chatRepository.updateConversationAfterMessage(
      conversation.id,
      conversation.conversationType,
      actor,
      command.content.slice(0, 120),
    );

    return toMessageDto(message);
  }

  async markConversationAsRead(auth: AuthContext, conversationId: string): Promise<ChatConversationDto> {
    const conversation = await this.findAccessibleConversation(auth, conversationId);
    const actor = this.resolveActor(auth);
    const actorId = this.resolveSenderId(auth, actor);

    const updated = await this.chatRepository.markConversationAsRead(conversation.id, actor, actorId);
    return toConversationDto(updated);
  }

  private buildConversationFilter(auth: AuthContext, query: ConversationQuery) {
    if (this.isAdmin(auth)) {
      return {
        page: query.page,
        limit: query.limit,
        conversationType: query.conversationType,
      };
    }

    if (auth.roles.includes(ROLES.MERCHANT)) {
      return {
        page: query.page,
        limit: query.limit,
        merchantId: auth.merchantId ?? auth.userId,
        conversationType: query.conversationType,
      };
    }

    if (auth.roles.includes(ROLES.SHIPPER)) {
      return {
        page: query.page,
        limit: query.limit,
        shipperId: auth.shipperId ?? auth.userId,
        conversationType: query.conversationType,
      };
    }

    return {
      page: query.page,
      limit: query.limit,
      customerId: auth.userId,
      conversationType: query.conversationType,
    };
  }

  private async findAccessibleConversation(auth: AuthContext, conversationId: string): Promise<ChatConversation> {
    const conversation = await this.chatRepository.findConversationById(conversationId);
    if (!conversation) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Conversation not found");
    }

    if (this.isAdmin(auth)) {
      return conversation;
    }

    const actor = this.resolveActor(auth);
    const actorId = this.resolveSenderId(auth, actor);

    const canAccess =
      (actor === ChatSenderRole.CUSTOMER && conversation.customerId === actorId) ||
      (actor === ChatSenderRole.MERCHANT && conversation.merchantId === actorId) ||
      (actor === ChatSenderRole.SHIPPER && conversation.shipperId === actorId);

    if (!canAccess) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "You do not have access to this conversation");
    }

    return conversation;
  }

  private assertConversationOwnership(auth: AuthContext, command: CreateConversationCommand) {
    if (this.isAdmin(auth)) {
      return;
    }

    if (auth.roles.includes(ROLES.CUSTOMER) && command.customerId !== auth.userId) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Customer cannot create conversation for another user");
    }

    if (auth.roles.includes(ROLES.MERCHANT)) {
      const merchantId = auth.merchantId ?? auth.userId;
      if (command.merchantId !== merchantId) {
        throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant cannot create conversation for another merchant");
      }
    }

    if (auth.roles.includes(ROLES.SHIPPER)) {
      const shipperId = auth.shipperId ?? auth.userId;
      if (command.shipperId && command.shipperId !== shipperId) {
        throw new ApiError(HTTP_STATUS.FORBIDDEN, "Shipper cannot create conversation for another shipper");
      }
    }
  }

  private resolveActor(auth: AuthContext): ChatSenderRole {
    if (auth.roles.includes(ROLES.ADMIN)) {
      return ChatSenderRole.SYSTEM;
    }

    if (auth.roles.includes(ROLES.MERCHANT)) {
      return ChatSenderRole.MERCHANT;
    }

    if (auth.roles.includes(ROLES.SHIPPER)) {
      return ChatSenderRole.SHIPPER;
    }

    return ChatSenderRole.CUSTOMER;
  }

  private resolveSenderId(auth: AuthContext, actor: ChatSenderRole): string {
    if (actor === ChatSenderRole.MERCHANT) {
      return auth.merchantId ?? auth.userId;
    }

    if (actor === ChatSenderRole.SHIPPER) {
      return auth.shipperId ?? auth.userId;
    }

    return auth.userId;
  }

  private isAdmin(auth: AuthContext) {
    return auth.roles.includes(ROLES.ADMIN);
  }
}
