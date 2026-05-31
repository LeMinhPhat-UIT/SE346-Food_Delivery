import { ChatConversation, ChatMessage } from "@prisma/client";
import {
  ChatConversationDto,
  ChatMessageDto,
  ChatConversationTypeDto,
  ChatMessageStatusDto,
  ChatMessageTypeDto,
  ChatSenderRoleDto,
} from "./chat.dto";

const mapConversationType = (value: ChatConversation["conversationType"]): ChatConversationTypeDto =>
  value as ChatConversationTypeDto;

const mapMessageType = (value: ChatMessage["messageType"]): ChatMessageTypeDto =>
  value as ChatMessageTypeDto;

const mapMessageStatus = (value: ChatMessage["status"]): ChatMessageStatusDto =>
  value as ChatMessageStatusDto;

const mapSenderRole = (value: ChatMessage["senderRole"]): ChatSenderRoleDto =>
  value as ChatSenderRoleDto;

export const toConversationDto = (conversation: ChatConversation): ChatConversationDto => ({
  id: conversation.id,
  conversationType: mapConversationType(conversation.conversationType),
  orderId: conversation.orderId,
  deliveryId: conversation.deliveryId,
  customerId: conversation.customerId,
  merchantId: conversation.merchantId,
  shipperId: conversation.shipperId,
  lastMessageAt: conversation.lastMessageAt,
  lastMessagePreview: conversation.lastMessagePreview,
  unreadCustomerCount: conversation.unreadCustomerCount,
  unreadMerchantCount: conversation.unreadMerchantCount,
  unreadShipperCount: conversation.unreadShipperCount,
  archivedAt: conversation.archivedAt,
  createdAt: conversation.createdAt,
  updatedAt: conversation.updatedAt,
});

export const toMessageDto = (message: ChatMessage): ChatMessageDto => ({
  id: message.id,
  conversationId: message.conversationId,
  senderRole: mapSenderRole(message.senderRole),
  senderId: message.senderId,
  content: message.content,
  messageType: mapMessageType(message.messageType),
  status: mapMessageStatus(message.status),
  readAt: message.readAt,
  createdAt: message.createdAt,
  updatedAt: message.updatedAt,
});
