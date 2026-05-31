export type ChatConversationTypeDto = "ORDER_MERCHANT" | "ORDER_SHIPPER";
export type ChatSenderRoleDto = "CUSTOMER" | "MERCHANT" | "SHIPPER" | "SYSTEM";
export type ChatMessageTypeDto = "TEXT" | "IMAGE" | "SYSTEM";
export type ChatMessageStatusDto = "SENT" | "DELIVERED" | "READ";

export interface ChatConversationDto {
  id: string;
  conversationType: ChatConversationTypeDto;
  orderId: string;
  deliveryId?: string | null;
  customerId: string;
  merchantId: string;
  shipperId?: string | null;
  lastMessageAt?: Date | null;
  lastMessagePreview?: string | null;
  unreadCustomerCount: number;
  unreadMerchantCount: number;
  unreadShipperCount: number;
  archivedAt?: Date | null;
  createdAt: Date;
  updatedAt: Date;
}

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  senderRole: ChatSenderRoleDto;
  senderId: string;
  content: string;
  messageType: ChatMessageTypeDto;
  status: ChatMessageStatusDto;
  readAt?: Date | null;
  createdAt: Date;
  updatedAt: Date;
}

export interface PaginatedChatResponse<T> {
  items: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
}
