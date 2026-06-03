const bearerAuth = {
  type: "http",
  scheme: "bearer",
  bearerFormat: "JWT",
  description: "Paste access token returned from Authentication Service login.",
};

export const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Order Service API",
    version: "1.0.0",
    description: "Swagger documentation for Order Service endpoints.",
  },
  servers: [
    { url: "http://localhost:8086", description: "Docker host port" },
    { url: "http://localhost:8080", description: "Local development port" },
  ],
  tags: [
    { name: "Health", description: "Service health check" },
    { name: "Cart", description: "Cart management" },
    { name: "Orders", description: "Order lifecycle and checkout" },
    { name: "Payments", description: "Payment and VNPay flow" },
    { name: "Vouchers", description: "Voucher management" },
  ],
  components: {
    securitySchemes: {
      bearerAuth,
    },
    schemas: {
      CartSelectedOptionRequest: {
        type: "object",
        required: ["optionId", "valueIds"],
        properties: {
          optionId: { type: "string", format: "uuid", example: "11111111-1111-1111-1111-111111111111" },
          valueIds: {
            type: "array",
            items: { type: "string", format: "uuid" },
            example: ["22222222-2222-2222-2222-222222222222"],
          },
        },
      },
      AddCartItemRequest: {
        type: "object",
        required: ["productId", "quantity"],
        properties: {
          productId: { type: "string", format: "uuid", example: "33333333-3333-3333-3333-333333333333" },
          quantity: { type: "integer", minimum: 1, maximum: 100, example: 2 },
          note: { type: "string", nullable: true, example: "Less sugar" },
          selectedOptions: {
            type: "array",
            items: { $ref: "#/components/schemas/CartSelectedOptionRequest" },
            example: [
              {
                optionId: "11111111-1111-1111-1111-111111111111",
                valueIds: ["22222222-2222-2222-2222-222222222222"],
              },
            ],
          },
        },
      },
      UpdateCartItemRequest: {
        type: "object",
        properties: {
          quantity: { type: "integer", minimum: 1, maximum: 100, example: 3 },
          note: { type: "string", nullable: true, example: "No ice" },
          selectedOptions: {
            type: "array",
            items: { $ref: "#/components/schemas/CartSelectedOptionRequest" },
          },
        },
      },
      CheckoutPreviewRequest: {
        type: "object",
        required: ["merchantId", "addressId"],
        properties: {
          merchantId: { type: "string", format: "uuid", example: "44444444-4444-4444-4444-444444444444" },
          addressId: { type: "string", format: "uuid", example: "55555555-5555-5555-5555-555555555555" },
          voucherCode: { type: "string", maxLength: 50, example: "SALE10" },
          paymentMethod: { type: "string", enum: ["COD", "VNPAY"], example: "VNPAY" },
        },
      },
      CreateOrderRequest: {
        type: "object",
        required: ["merchantId", "addressId", "paymentMethod"],
        properties: {
          merchantId: { type: "string", format: "uuid", example: "44444444-4444-4444-4444-444444444444" },
          addressId: { type: "string", format: "uuid", example: "55555555-5555-5555-5555-555555555555" },
          voucherCode: { type: "string", maxLength: 50, example: "SALE10" },
          paymentMethod: { type: "string", enum: ["COD", "VNPAY"], example: "VNPAY" },
          note: { type: "string", maxLength: 1000, example: "Call me when you arrive" },
        },
      },
      UpdateOrderStatusRequest: {
        type: "object",
        required: ["status"],
        properties: {
          status: {
            type: "string",
            enum: ["CONFIRMED", "PREPARING", "READY", "CANCELLED"],
            example: "CONFIRMED",
          },
          note: { type: "string", maxLength: 1000, example: "Kitchen started" },
          cancelReason: { type: "string", maxLength: 1000, example: "Item unavailable" },
        },
      },
      CancelOrderRequest: {
        type: "object",
        required: ["cancelReason"],
        properties: {
          cancelReason: { type: "string", maxLength: 1000, example: "Changed my mind" },
        },
      },
      MyOrdersQuery: {
        type: "object",
        properties: {
          page: { type: "integer", minimum: 1, example: 1 },
          limit: { type: "integer", minimum: 1, maximum: 100, example: 10 },
          merchantId: { type: "string", format: "uuid" },
          status: {
            type: "string",
            enum: ["PENDING", "CONFIRMED", "PREPARING", "READY", "PICKED_UP", "DELIVERING", "DELIVERED", "CANCELLED"],
          },
          paymentStatus: { type: "string", enum: ["PENDING", "PAID", "FAILED", "REFUNDED"] },
          sortBy: { type: "string", enum: ["createdAt", "totalAmount"], example: "createdAt" },
          sortOrder: { type: "string", enum: ["asc", "desc"], example: "desc" },
        },
      },
      CreateVnpayPaymentUrlRequest: {
        type: "object",
        properties: {
          bankCode: { type: "string", maxLength: 20, example: "NCB" },
        },
      },
      VoucherQuery: {
        type: "object",
        properties: {
          page: { type: "integer", minimum: 1, example: 1 },
          limit: { type: "integer", minimum: 1, maximum: 100, example: 10 },
          search: { type: "string", example: "SALE" },
          merchantId: { type: "string", format: "uuid" },
          isActive: { type: "boolean" },
          includeDeleted: { type: "boolean" },
          discountType: { type: "string", enum: ["PERCENTAGE", "FIXED"] },
          discountTarget: { type: "string", enum: ["SUBTOTAL", "DELIVERY_FEE"] },
          availability: { type: "string", enum: ["active", "upcoming", "expired", "inactive"] },
          sortBy: {
            type: "string",
            enum: ["createdAt", "startDate", "endDate", "code", "name", "discountValue", "usedCount"],
          },
          sortOrder: { type: "string", enum: ["asc", "desc"] },
        },
      },
      VoucherCreateRequest: {
        type: "object",
        required: ["code", "name", "discountType", "discountValue", "startDate", "endDate"],
        properties: {
          code: { type: "string", maxLength: 50, example: "SALE10" },
          name: { type: "string", maxLength: 255, example: "New Year Sale" },
          description: { type: "string", nullable: true, maxLength: 2000, example: "10% off for New Year" },
          discountType: { type: "string", enum: ["PERCENTAGE", "FIXED"], example: "PERCENTAGE" },
          discountValue: { type: "number", example: 10 },
          maxDiscount: { type: "number", nullable: true, example: 50000 },
          minOrderAmount: { type: "number", nullable: true, example: 100000 },
          discountTarget: { type: "string", enum: ["SUBTOTAL", "DELIVERY_FEE"], example: "SUBTOTAL" },
          merchantId: { type: "string", format: "uuid", nullable: true, example: null },
          usageLimit: { type: "integer", nullable: true, example: 100 },
          perUserLimit: { type: "integer", example: 1 },
          startDate: { type: "string", format: "date-time", example: "2026-06-01T00:00:00.000Z" },
          endDate: { type: "string", format: "date-time", example: "2026-06-30T23:59:59.000Z" },
          isActive: { type: "boolean", example: true },
        },
      },
      VoucherUpdateRequest: {
        type: "object",
        properties: {
          code: { type: "string", maxLength: 50, example: "SALE10" },
          name: { type: "string", maxLength: 255, example: "New Year Sale" },
          description: { type: "string", nullable: true, maxLength: 2000, example: "10% off for New Year" },
          discountType: { type: "string", enum: ["PERCENTAGE", "FIXED"], example: "PERCENTAGE" },
          discountValue: { type: "number", example: 10 },
          maxDiscount: { type: "number", nullable: true, example: 50000 },
          minOrderAmount: { type: "number", nullable: true, example: 100000 },
          discountTarget: { type: "string", enum: ["SUBTOTAL", "DELIVERY_FEE"], example: "SUBTOTAL" },
          merchantId: { type: "string", format: "uuid", nullable: true, example: null },
          usageLimit: { type: "integer", nullable: true, example: 100 },
          perUserLimit: { type: "integer", example: 1 },
          startDate: { type: "string", format: "date-time", example: "2026-06-01T00:00:00.000Z" },
          endDate: { type: "string", format: "date-time", example: "2026-06-30T23:59:59.000Z" },
          isActive: { type: "boolean", example: true },
        },
      },
      VoucherStatusRequest: {
        type: "object",
        required: ["isActive"],
        properties: {
          isActive: { type: "boolean", example: true },
        },
      },
      VoucherValidateRequest: {
        type: "object",
        required: ["code", "userId", "subtotal"],
        properties: {
          code: { type: "string", maxLength: 50, example: "SALE10" },
          userId: { type: "string", format: "uuid", example: "66666666-6666-6666-6666-666666666666" },
          merchantId: { type: "string", format: "uuid", example: "44444444-4444-4444-4444-444444444444" },
          subtotal: { type: "number", minimum: 0, example: 250000 },
          deliveryFee: { type: "number", minimum: 0, example: 15000 },
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
          "200": {
            description: "Service is healthy",
            content: {
              "text/plain": {
                schema: { type: "string", example: "Order Service is running healthy!" },
              },
            },
          },
        },
      },
    },
    "/api/orders/cart": {
      get: {
        tags: ["Cart"],
        summary: "Get my carts",
        security: [{ bearerAuth: [] }],
        responses: { "200": { description: "Cart list" } },
      },
      delete: {
        tags: ["Cart"],
        summary: "Clear all carts",
        security: [{ bearerAuth: [] }],
        responses: { "200": { description: "All carts cleared" } },
      },
    },
    "/api/orders/cart/merchant/{merchantId}": {
      get: {
        tags: ["Cart"],
        summary: "Get cart by merchant",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "merchantId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Merchant cart" } },
      },
      delete: {
        tags: ["Cart"],
        summary: "Clear cart by merchant",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "merchantId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Merchant cart cleared" } },
      },
    },
    "/api/orders/cart/items": {
      post: {
        tags: ["Cart"],
        summary: "Add item to cart",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/AddCartItemRequest" },
            },
          },
        },
        responses: { "201": { description: "Cart item added" } },
      },
    },
    "/api/orders/cart/items/{itemId}": {
      patch: {
        tags: ["Cart"],
        summary: "Update cart item",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "itemId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/UpdateCartItemRequest" },
            },
          },
        },
        responses: { "200": { description: "Cart item updated" } },
      },
      delete: {
        tags: ["Cart"],
        summary: "Remove cart item",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "itemId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Cart item removed" } },
      },
    },
    "/api/orders/checkout/preview": {
      post: {
        tags: ["Orders"],
        summary: "Checkout preview",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CheckoutPreviewRequest" },
            },
          },
        },
        responses: { "200": { description: "Checkout preview result" } },
      },
    },
    "/api/orders/my": {
      get: {
        tags: ["Orders"],
        summary: "Get my orders",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 10 } },
          { name: "merchantId", in: "query", schema: { type: "string", format: "uuid" } },
          { name: "status", in: "query", schema: { type: "string" } },
          { name: "paymentStatus", in: "query", schema: { type: "string" } },
          { name: "sortBy", in: "query", schema: { type: "string" } },
          { name: "sortOrder", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Order history list" } },
      },
      post: {
        tags: ["Orders"],
        summary: "Create order",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CreateOrderRequest" },
            },
          },
        },
        responses: { "201": { description: "Order created" } },
      },
    },
    "/api/orders/merchant/my": {
      get: {
        tags: ["Orders"],
        summary: "Get merchant orders",
        security: [{ bearerAuth: [] }],
        responses: { "200": { description: "Merchant orders" } },
      },
    },
    "/api/orders/merchant/my/{id}": {
      get: {
        tags: ["Orders"],
        summary: "Get merchant order by id",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Merchant order detail" } },
      },
    },
    "/api/orders/merchant/my/{id}/status": {
      patch: {
        tags: ["Orders"],
        summary: "Update merchant order status",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/UpdateOrderStatusRequest" },
            },
          },
        },
        responses: { "200": { description: "Order status updated" } },
      },
    },
    "/api/orders/{id}": {
      get: {
        tags: ["Orders"],
        summary: "Get order by id",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Order detail" } },
      },
    },
    "/api/orders/{id}/cancel": {
      patch: {
        tags: ["Orders"],
        summary: "Cancel my order",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CancelOrderRequest" },
            },
          },
        },
        responses: { "200": { description: "Order cancelled" } },
      },
    },
    "/api/orders/payments/{orderId}": {
      get: {
        tags: ["Payments"],
        summary: "Get payment by order id",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "orderId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Payment detail" } },
      },
    },
    "/api/orders/payments/{orderId}/vnpay/url": {
      post: {
        tags: ["Payments"],
        summary: "Create VNPay payment URL",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "orderId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: false,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CreateVnpayPaymentUrlRequest" },
            },
          },
        },
        responses: { "200": { description: "VNPay payment URL created" } },
      },
    },
    "/api/orders/payments/vnpay/return": {
      get: {
        tags: ["Payments"],
        summary: "VNPay return callback",
        parameters: [
          { name: "vnp_Amount", in: "query", schema: { type: "string" } },
          { name: "vnp_BankCode", in: "query", schema: { type: "string" } },
          { name: "vnp_ResponseCode", in: "query", schema: { type: "string" } },
          { name: "vnp_SecureHash", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "VNPay return handled" } },
      },
    },
    "/api/orders/payments/vnpay/ipn": {
      get: {
        tags: ["Payments"],
        summary: "VNPay IPN callback",
        parameters: [
          { name: "vnp_Amount", in: "query", schema: { type: "string" } },
          { name: "vnp_BankCode", in: "query", schema: { type: "string" } },
          { name: "vnp_ResponseCode", in: "query", schema: { type: "string" } },
          { name: "vnp_SecureHash", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "VNPay IPN handled" } },
      },
    },
    "/api/orders/vouchers": {
      get: {
        tags: ["Vouchers"],
        summary: "List vouchers",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, maximum: 100, default: 10 } },
          { name: "search", in: "query", schema: { type: "string" } },
          { name: "merchantId", in: "query", schema: { type: "string", format: "uuid" } },
          { name: "isActive", in: "query", schema: { type: "boolean" } },
          { name: "includeDeleted", in: "query", schema: { type: "boolean" } },
          { name: "discountType", in: "query", schema: { type: "string" } },
          { name: "discountTarget", in: "query", schema: { type: "string" } },
          { name: "availability", in: "query", schema: { type: "string" } },
          { name: "sortBy", in: "query", schema: { type: "string" } },
          { name: "sortOrder", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Voucher list" } },
      },
      post: {
        tags: ["Vouchers"],
        summary: "Create voucher",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoucherCreateRequest" },
            },
          },
        },
        responses: { "201": { description: "Voucher created" } },
      },
    },
    "/api/orders/vouchers/validate": {
      post: {
        tags: ["Vouchers"],
        summary: "Validate voucher",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoucherValidateRequest" },
            },
          },
        },
        responses: { "200": { description: "Voucher validated" } },
      },
    },
    "/api/orders/vouchers/code/{code}": {
      get: {
        tags: ["Vouchers"],
        summary: "Get voucher by code",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "code", in: "path", required: true, schema: { type: "string" } }],
        responses: { "200": { description: "Voucher detail" } },
      },
    },
    "/api/orders/vouchers/{id}": {
      get: {
        tags: ["Vouchers"],
        summary: "Get voucher by id",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Voucher detail" } },
      },
      put: {
        tags: ["Vouchers"],
        summary: "Update voucher",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoucherUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Voucher updated" } },
      },
      patch: {
        tags: ["Vouchers"],
        summary: "Patch voucher",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoucherUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Voucher patched" } },
      },
      delete: {
        tags: ["Vouchers"],
        summary: "Delete voucher",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Voucher deleted" } },
      },
    },
    "/api/orders/vouchers/{id}/status": {
      patch: {
        tags: ["Vouchers"],
        summary: "Update voucher status",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/VoucherStatusRequest" },
            },
          },
        },
        responses: { "200": { description: "Voucher status updated" } },
      },
    },
    "/api/orders/vouchers/{id}/restore": {
      patch: {
        tags: ["Vouchers"],
        summary: "Restore voucher",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Voucher restored" } },
      },
    },
  },
} as const;
