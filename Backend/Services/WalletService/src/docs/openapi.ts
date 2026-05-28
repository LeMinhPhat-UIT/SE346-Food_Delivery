const bearerAuth = {
  type: "http",
  scheme: "bearer",
  bearerFormat: "JWT",
  description: "Paste access token returned from Authentication Service login.",
};

export const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Wallet Service API",
    version: "1.0.0",
    description: "Swagger documentation for Wallet Service endpoints.",
  },
  servers: [
    { url: "http://localhost:8089", description: "Docker host port" },
    { url: "http://localhost:8080", description: "Local development port" },
  ],
  tags: [
    { name: "Health", description: "Service health check" },
    { name: "Wallet", description: "Wallet details and transactions" },
    { name: "Topup", description: "Wallet topup flow with VNPay" },
    { name: "Admin", description: "Admin wallet monitoring" },
  ],
  components: {
    securitySchemes: {
      bearerAuth,
    },
    schemas: {
      WalletOwnerType: {
        type: "string",
        enum: ["MERCHANT", "SHIPPER", "ADMIN"],
      },
      ListQuery: {
        type: "object",
        properties: {
          page: { type: "integer", minimum: 1, example: 1 },
          limit: { type: "integer", minimum: 1, maximum: 100, example: 20 },
        },
      },
      TopupRequest: {
        type: "object",
        required: ["amount"],
        properties: {
          amount: { type: "number", exclusiveMinimum: 0, example: 100000 },
          bankCode: { type: "string", maxLength: 20, example: "NCB" },
        },
      },
      WalletResponse: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
          ownerType: { $ref: "#/components/schemas/WalletOwnerType" },
          ownerId: { type: "string", example: "merchant-or-shipper-user-id" },
          balance: { type: "number", example: 150000 },
          negativeSince: { type: "string", format: "date-time", nullable: true },
          currency: { type: "string", example: "VND" },
          isActive: { type: "boolean", example: true },
          createdAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" },
        },
      },
      WalletTransactionResponse: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
          walletId: { type: "string", format: "uuid" },
          type: {
            type: "string",
            enum: ["topup", "payment", "refund", "withdrawal", "commission", "adjustment"],
          },
          amount: { type: "number" },
          balanceBefore: { type: "number" },
          balanceAfter: { type: "number" },
          referenceId: { type: "string", format: "uuid", nullable: true },
          referenceType: { type: "string", nullable: true },
          referenceCode: { type: "string", nullable: true },
          description: { type: "string", nullable: true },
          status: {
            type: "string",
            enum: ["pending", "completed", "failed", "reversed"],
          },
          idempotencyKey: { type: "string", nullable: true },
          metadata: {},
          createdAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" },
        },
      },
      WalletTopupResponse: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
          walletId: { type: "string", format: "uuid" },
          ownerType: { $ref: "#/components/schemas/WalletOwnerType" },
          ownerId: { type: "string", example: "merchant-or-shipper-user-id" },
          requestCode: { type: "string", example: "TOPUP-20260528-0001" },
          amount: { type: "number", example: 100000 },
          provider: { type: "string", example: "VNPAY" },
          bankCode: { type: "string", nullable: true, example: "NCB" },
          status: {
            type: "string",
            enum: ["pending", "completed", "failed", "cancelled"],
          },
          transactionId: { type: "string", nullable: true },
          paymentData: {},
          expiresAt: { type: "string", format: "date-time" },
          paidAt: { type: "string", format: "date-time", nullable: true },
          createdAt: { type: "string", format: "date-time" },
          updatedAt: { type: "string", format: "date-time" },
        },
      },
      CreateTopupPaymentUrlResponse: {
        type: "object",
        properties: {
          topupId: { type: "string", format: "uuid" },
          requestCode: { type: "string", example: "TOPUP-20260528-0001" },
          amount: { type: "number", example: 100000 },
          expiresAt: { type: "string", format: "date-time" },
          paymentUrl: { type: "string", format: "uri" },
        },
      },
      NegativeWalletResponse: {
        allOf: [
          { $ref: "#/components/schemas/WalletResponse" },
          {
            type: "object",
            properties: {
              negativeDays: { type: "number", example: 3 },
            },
          },
        ],
      },
    },
  },
  paths: {
    "/health": {
      get: {
        tags: ["Health"],
        summary: "Health check",
        responses: {
          "200": {
            description: "Service is healthy",
            content: {
              "text/plain": {
                schema: { type: "string", example: "Wallet Service is running healthy!" },
              },
            },
          },
        },
      },
    },
    "/api/wallets/me": {
      get: {
        tags: ["Wallet"],
        summary: "Get my wallet",
        security: [{ bearerAuth: [] }],
        responses: { "200": { description: "Wallet detail" } },
      },
    },
    "/api/wallets/me/transactions": {
      get: {
        tags: ["Wallet"],
        summary: "Get my transactions",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 20 } },
        ],
        responses: { "200": { description: "Transaction list" } },
      },
    },
    "/api/wallets/me/transactions/order/{orderId}": {
      get: {
        tags: ["Wallet"],
        summary: "Get my transactions by order id",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "orderId", in: "path", required: true, schema: { type: "string", format: "uuid" } },
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 20 } },
        ],
        responses: { "200": { description: "Transaction list by order" } },
      },
    },
    "/api/wallets/me/transactions/reference/{referenceType}/{referenceId}": {
      get: {
        tags: ["Wallet"],
        summary: "Get my transactions by reference",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "referenceType", in: "path", required: true, schema: { type: "string" } },
          { name: "referenceId", in: "path", required: true, schema: { type: "string", format: "uuid" } },
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 20 } },
        ],
        responses: { "200": { description: "Transaction list by reference" } },
      },
    },
    "/api/wallets/me/topup/vnpay/url": {
      post: {
        tags: ["Topup"],
        summary: "Create wallet topup VNPay URL",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/TopupRequest" },
            },
          },
        },
        responses: { "200": { description: "Topup payment URL created" } },
      },
    },
    "/api/wallets/me/topups": {
      get: {
        tags: ["Topup"],
        summary: "Get my topups",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 20 } },
        ],
        responses: { "200": { description: "Topup list" } },
      },
    },
    "/api/wallets/me/topups/{topupId}": {
      get: {
        tags: ["Topup"],
        summary: "Get my topup by id",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "topupId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Topup detail" } },
      },
    },
    "/api/wallets/topup/vnpay/return": {
      get: {
        tags: ["Topup"],
        summary: "VNPay topup return callback",
        parameters: [
          { name: "vnp_Amount", in: "query", schema: { type: "string" } },
          { name: "vnp_BankCode", in: "query", schema: { type: "string" } },
          { name: "vnp_ResponseCode", in: "query", schema: { type: "string" } },
          { name: "vnp_SecureHash", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Topup return handled" } },
      },
    },
    "/api/wallets/topup/vnpay/ipn": {
      get: {
        tags: ["Topup"],
        summary: "VNPay topup IPN callback",
        parameters: [
          { name: "vnp_Amount", in: "query", schema: { type: "string" } },
          { name: "vnp_BankCode", in: "query", schema: { type: "string" } },
          { name: "vnp_ResponseCode", in: "query", schema: { type: "string" } },
          { name: "vnp_SecureHash", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Topup IPN handled" } },
      },
    },
    "/api/wallets/admin/owners/{ownerType}/{ownerId}": {
      get: {
        tags: ["Admin"],
        summary: "Get wallet by owner",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "ownerType", in: "path", required: true, schema: { type: "string", enum: ["MERCHANT", "SHIPPER", "ADMIN"] } },
          { name: "ownerId", in: "path", required: true, schema: { type: "string", format: "uuid" } },
        ],
        responses: { "200": { description: "Wallet detail" } },
      },
    },
    "/api/wallets/admin/negative": {
      get: {
        tags: ["Admin"],
        summary: "Get negative wallets",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 20 } },
        ],
        responses: { "200": { description: "Negative wallet list" } },
      },
    },
  },
} as const;
