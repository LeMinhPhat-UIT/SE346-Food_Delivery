export const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Chat Service API",
    version: "1.0.0",
    description: "Chat between customer-merchant and customer-shipper.",
  },
  servers: [
    {
      url: "http://localhost:8090",
      description: "Local development server",
    },
  ],
  tags: [
    { name: "Health", description: "Health checks" },
    { name: "Chats", description: "Conversation and message APIs" },
  ],
  components: {
    securitySchemes: {
      BearerAuth: {
        type: "http",
        scheme: "bearer",
        bearerFormat: "JWT",
      },
    },
    schemas: {
      ChatConversationType: {
        type: "string",
        enum: ["ORDER_MERCHANT", "ORDER_SHIPPER"],
      },
      ChatMessageType: {
        type: "string",
        enum: ["TEXT", "IMAGE", "SYSTEM"],
      },
      CreateConversationRequest: {
        type: "object",
        required: ["conversationType", "orderId", "customerId", "merchantId"],
        properties: {
          conversationType: { $ref: "#/components/schemas/ChatConversationType" },
          orderId: { type: "string" },
          deliveryId: { type: "string", nullable: true },
          customerId: { type: "string" },
          merchantId: { type: "string" },
          shipperId: { type: "string", nullable: true },
        },
      },
      CreateMessageRequest: {
        type: "object",
        required: ["content"],
        properties: {
          content: { type: "string", example: "Xin chào, đơn của mình thế nào rồi?" },
          messageType: { $ref: "#/components/schemas/ChatMessageType" },
        },
      },
      ChatConversation: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
          conversationType: { $ref: "#/components/schemas/ChatConversationType" },
          orderId: { type: "string" },
          deliveryId: { type: "string", nullable: true },
          customerId: { type: "string" },
          merchantId: { type: "string" },
          shipperId: { type: "string", nullable: true },
          lastMessageAt: { type: "string", format: "date-time", nullable: true },
          lastMessagePreview: { type: "string", nullable: true },
          unreadCustomerCount: { type: "integer" },
          unreadMerchantCount: { type: "integer" },
          unreadShipperCount: { type: "integer" },
          createdAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" },
        },
      },
      ChatMessage: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
          conversationId: { type: "string", format: "uuid" },
          senderRole: { type: "string", enum: ["CUSTOMER", "MERCHANT", "SHIPPER", "SYSTEM"] },
          senderId: { type: "string" },
          content: { type: "string" },
          messageType: { $ref: "#/components/schemas/ChatMessageType" },
          status: { type: "string", enum: ["SENT", "DELIVERED", "READ"] },
          readAt: { type: "string", format: "date-time", nullable: true },
          createdAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" },
        },
      },
      PaginatedChatConversationResponse: {
        type: "object",
        properties: {
          items: {
            type: "array",
            items: { $ref: "#/components/schemas/ChatConversation" },
          },
          pagination: {
            type: "object",
            properties: {
              page: { type: "integer" },
              limit: { type: "integer" },
              total: { type: "integer" },
              totalPages: { type: "integer" },
            },
          },
        },
      },
      PaginatedChatMessageResponse: {
        type: "object",
        properties: {
          items: {
            type: "array",
            items: { $ref: "#/components/schemas/ChatMessage" },
          },
          pagination: {
            type: "object",
            properties: {
              page: { type: "integer" },
              limit: { type: "integer" },
              total: { type: "integer" },
              totalPages: { type: "integer" },
            },
          },
        },
      },
    },
  },
  paths: {
    "/health": {
      get: {
        tags: ["Health"],
        summary: "Health check",
        responses: {
          200: {
            description: "Service is healthy",
          },
        },
      },
    },
    "/api/chats/conversations": {
      get: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        summary: "List conversations",
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", default: 20 } },
          { name: "conversationType", in: "query", schema: { $ref: "#/components/schemas/ChatConversationType" } },
        ],
        responses: {
          200: {
            description: "Conversations fetched successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/PaginatedChatConversationResponse" },
              },
            },
          },
        },
      },
      post: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        summary: "Create conversation",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CreateConversationRequest" },
            },
          },
        },
        responses: {
          201: {
            description: "Conversation created successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/ChatConversation" },
              },
            },
          },
        },
      },
    },
    "/api/chats/conversations/{conversationId}": {
      get: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "conversationId",
            in: "path",
            required: true,
            schema: { type: "string", format: "uuid" },
          },
        ],
        summary: "Get conversation detail",
        responses: {
          200: {
            description: "Conversation fetched successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/ChatConversation" },
              },
            },
          },
        },
      },
    },
    "/api/chats/orders/{orderId}/{conversationType}": {
      get: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "orderId",
            in: "path",
            required: true,
            schema: { type: "string" },
          },
          {
            name: "conversationType",
            in: "path",
            required: true,
            schema: { $ref: "#/components/schemas/ChatConversationType" },
          },
        ],
        summary: "Get conversation by order and type",
        responses: {
          200: {
            description: "Conversation fetched successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/ChatConversation" },
              },
            },
          },
        },
      },
    },
    "/api/chats/conversations/{conversationId}/messages": {
      get: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "conversationId",
            in: "path",
            required: true,
            schema: { type: "string", format: "uuid" },
          },
          { name: "page", in: "query", schema: { type: "integer", default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", default: 20 } },
        ],
        summary: "Get messages in a conversation",
        responses: {
          200: {
            description: "Messages fetched successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/PaginatedChatMessageResponse" },
              },
            },
          },
        },
      },
      post: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "conversationId",
            in: "path",
            required: true,
            schema: { type: "string", format: "uuid" },
          },
        ],
        summary: "Send a message",
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CreateMessageRequest" },
            },
          },
        },
        responses: {
          201: {
            description: "Message sent successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/ChatMessage" },
              },
            },
          },
        },
      },
    },
    "/api/chats/conversations/{conversationId}/read": {
      patch: {
        tags: ["Chats"],
        security: [{ BearerAuth: [] }],
        parameters: [
          {
            name: "conversationId",
            in: "path",
            required: true,
            schema: { type: "string", format: "uuid" },
          },
        ],
        summary: "Mark conversation as read",
        responses: {
          200: {
            description: "Conversation marked as read successfully",
            content: {
              "application/json": {
                schema: { $ref: "#/components/schemas/ChatConversation" },
              },
            },
          },
        },
      },
    },
  },
};
