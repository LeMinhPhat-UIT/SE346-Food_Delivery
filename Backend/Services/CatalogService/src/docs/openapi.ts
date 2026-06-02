const bearerAuth = {
  type: "http",
  scheme: "bearer",
  bearerFormat: "JWT",
  description: "Paste access token returned from Authentication Service login.",
};

export const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Catalog Service API",
    version: "1.0.0",
    description: "Swagger documentation for Catalog Service endpoints.",
  },
  servers: [
    {
      url: "http://localhost:8085",
      description: "Docker host port",
    },
    {
      url: "http://localhost:8080",
      description: "Local development port",
    },
  ],
  tags: [
    { name: "Health", description: "Service health check" },
    { name: "Categories", description: "Category management" },
    { name: "Products", description: "Product management" },
    { name: "Reviews", description: "Review management" },
    { name: "Uploads", description: "File upload and deletion" },
  ],
  components: {
    securitySchemes: {
      bearerAuth,
    },
    schemas: {
      ApiResponse: {
        type: "object",
        properties: {
          ok: { type: "boolean", example: true },
          message: { type: "string", example: "Success" },
          data: {},
        },
      },
      CategoryIdParam: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
        },
        required: ["id"],
      },
      ProductIdParam: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
        },
        required: ["id"],
      },
      ReviewIdParam: {
        type: "object",
        properties: {
          id: { type: "string", format: "uuid" },
        },
        required: ["id"],
      },
      CategoryCreateRequest: {
        type: "object",
        required: ["name"],
        properties: {
          name: { type: "string", example: "Beverages" },
          description: { type: "string", nullable: true, example: "Drinks and beverages" },
          iconUrl: { type: "string", nullable: true, example: "https://cdn.example.com/category/icon.png" },
          parentId: { type: "string", format: "uuid", nullable: true, example: null },
          sortOrder: { type: "integer", example: 0 },
          isActive: { type: "boolean", example: true },
        },
      },
      CategoryUpdateRequest: {
        allOf: [
          { $ref: "#/components/schemas/CategoryCreateRequest" },
          {
            description: "At least one field is required.",
          },
        ],
      },
      CategoryStatusRequest: {
        type: "object",
        required: ["isActive"],
        properties: {
          isActive: { type: "boolean", example: true },
        },
      },
      ProductOptionValueRequest: {
        type: "object",
        required: ["name"],
        properties: {
          name: { type: "string", example: "Large" },
          additionalPrice: { type: "number", example: 5000 },
          isAvailable: { type: "boolean", example: true },
        },
      },
      ProductOptionRequest: {
        type: "object",
        required: ["name"],
        properties: {
          categoryId: { type: "string", format: "uuid", nullable: true, example: null },
          name: { type: "string", example: "Size" },
          isRequired: { type: "boolean", example: false },
          maxSelections: { type: "integer", example: 1 },
          values: {
            type: "array",
            items: { $ref: "#/components/schemas/ProductOptionValueRequest" },
            example: [
              { name: "Small", additionalPrice: 0, isAvailable: true },
              { name: "Large", additionalPrice: 5000, isAvailable: true },
            ],
          },
        },
      },
      ProductCreateRequest: {
        type: "object",
        required: ["merchantId", "name", "basePrice"],
        properties: {
          merchantId: { type: "string", example: "merchant-user-id-or-uuid" },
          categoryId: { type: "string", format: "uuid", nullable: true, example: null },
          taxonomy: {
            type: "string",
            enum: ["FOOD", "DRINK", "DESSERT", "OTHER"],
            example: "DRINK",
          },
          name: { type: "string", example: "Vietnamese Milk Coffee" },
          description: { type: "string", nullable: true, example: "Traditional coffee with condensed milk" },
          imageUrl: { type: "string", nullable: true, example: "https://cdn.example.com/product/coffee.jpg" },
          basePrice: { type: "number", example: 29000 },
          discountPrice: { type: "number", nullable: true, example: 25000 },
          isAvailable: { type: "boolean", example: true },
          isFeatured: { type: "boolean", example: false },
          prepTime: { type: "integer", nullable: true, example: 10 },
          options: {
            type: "array",
            items: { $ref: "#/components/schemas/ProductOptionRequest" },
            example: [
              {
                name: "Size",
                isRequired: true,
                maxSelections: 1,
                values: [
                  { name: "Small", additionalPrice: 0, isAvailable: true },
                  { name: "Large", additionalPrice: 5000, isAvailable: true },
                ],
              },
            ],
          },
        },
      },
      ProductUpdateRequest: {
        allOf: [
          { $ref: "#/components/schemas/ProductCreateRequest" },
          {
            description: "At least one field is required.",
          },
        ],
      },
      ProductAvailabilityRequest: {
        type: "object",
        required: ["isAvailable"],
        properties: {
          isAvailable: { type: "boolean", example: true },
        },
      },
      ProductFeaturedRequest: {
        type: "object",
        required: ["isFeatured"],
        properties: {
          isFeatured: { type: "boolean", example: true },
        },
      },
      ProductBatchAvailabilityRequest: {
        type: "object",
        required: ["productIds", "isAvailable"],
        properties: {
          productIds: {
            type: "array",
            items: { type: "string", format: "uuid" },
            example: ["11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222"],
          },
          isAvailable: { type: "boolean", example: true },
        },
      },
      ReviewCreateRequest: {
        type: "object",
        required: ["orderId", "rating"],
        description: "Authenticated user is derived from the bearer token; do not send userId.",
        properties: {
          orderId: { type: "string", format: "uuid", example: "33333333-3333-3333-3333-333333333333" },
          merchantId: { type: "string", format: "uuid", nullable: true, example: null },
          productId: { type: "string", format: "uuid", nullable: true, example: null },
          shipperId: { type: "string", format: "uuid", nullable: true, example: null },
          rating: { type: "integer", minimum: 1, maximum: 5, example: 5 },
          comment: { type: "string", nullable: true, example: "Great food and fast delivery!" },
          images: {
            type: "array",
            items: { type: "string", format: "uri" },
            nullable: true,
            example: ["https://cdn.example.com/review/1.png"],
          },
          merchantReply: { type: "string", nullable: true, example: null },
          repliedAt: { type: "string", format: "date-time", nullable: true, example: null },
        },
      },
      ReviewUpdateRequest: {
        type: "object",
        description: "Only mutable review fields are allowed here.",
        properties: {
          rating: { type: "integer", minimum: 1, maximum: 5, example: 5 },
          comment: { type: "string", nullable: true, example: "Updated review text" },
          images: {
            type: "array",
            nullable: true,
            items: { type: "string", format: "uri" },
            example: ["https://cdn.example.com/review/1.png"],
          },
        },
      },
      ReviewReplyRequest: {
        type: "object",
        required: ["merchantReply"],
        properties: {
          merchantReply: { type: "string", example: "Thank you for your review!" },
        },
      },
      UploadRequest: {
        type: "object",
        required: ["entityType", "files"],
        properties: {
          entityType: { type: "string", enum: ["category", "product", "review"], example: "product" },
          entityId: { type: "string", format: "uuid", nullable: true, example: null },
          files: {
            type: "array",
            items: { type: "string", format: "binary" },
          },
        },
      },
      DeleteUploadRequest: {
        type: "object",
        required: ["paths"],
        properties: {
          paths: {
            type: "array",
            items: { type: "string" },
            example: [
              "catalog/product/123/image1.png",
              "catalog/product/123/image2.png",
            ],
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
          "200": {
            description: "Service is healthy",
            content: {
              "text/plain": {
                schema: { type: "string", example: "Catalog Service is running healthy!" },
              },
            },
          },
        },
      },
    },
    "/api/catalog/categories": {
      get: {
        tags: ["Categories"],
        summary: "List categories",
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, default: 20 } },
          { name: "search", in: "query", schema: { type: "string" } },
          { name: "status", in: "query", schema: { type: "string", example: "ACTIVE" } },
        ],
        responses: { "200": { description: "Paginated category list" } },
      },
      post: {
        tags: ["Categories"],
        summary: "Create category",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CategoryCreateRequest" },
            },
          },
        },
        responses: { "201": { description: "Category created" } },
      },
    },
    "/api/catalog/categories/tree": {
      get: {
        tags: ["Categories"],
        summary: "Get category tree",
        responses: { "200": { description: "Nested category tree" } },
      },
    },
    "/api/catalog/categories/root": {
      get: {
        tags: ["Categories"],
        summary: "Get root categories",
        responses: { "200": { description: "Root categories" } },
      },
    },
    "/api/catalog/categories/{id}": {
      get: {
        tags: ["Categories"],
        summary: "Get category by id",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Category detail" } },
      },
      put: {
        tags: ["Categories"],
        summary: "Update category",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CategoryUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Category updated" } },
      },
      patch: {
        tags: ["Categories"],
        summary: "Patch category",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CategoryUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Category patched" } },
      },
      delete: {
        tags: ["Categories"],
        summary: "Delete category",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Category deleted" } },
      },
    },
    "/api/catalog/categories/{id}/status": {
      patch: {
        tags: ["Categories"],
        summary: "Update category status",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/CategoryStatusRequest" },
            },
          },
        },
        responses: { "200": { description: "Category status updated" } },
      },
    },
    "/api/catalog/categories/{id}/restore": {
      patch: {
        tags: ["Categories"],
        summary: "Restore category",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Category restored" } },
      },
    },
    "/api/catalog/products": {
      get: {
        tags: ["Products"],
        summary: "List products",
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, default: 20 } },
          { name: "search", in: "query", schema: { type: "string" } },
          { name: "merchantId", in: "query", schema: { type: "string" } },
          { name: "categoryId", in: "query", schema: { type: "string" } },
          {
            name: "taxonomy",
            in: "query",
            schema: { type: "string", enum: ["FOOD", "DRINK", "DESSERT", "OTHER"] },
          },
          { name: "status", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Paginated product list" } },
      },
      post: {
        tags: ["Products"],
        summary: "Create product",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductCreateRequest" },
            },
          },
        },
        responses: { "201": { description: "Product created" } },
      },
    },
    "/api/catalog/products/merchant/me": {
      get: {
        tags: ["Products"],
        summary: "Get my merchant products",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, default: 20 } },
          { name: "search", in: "query", schema: { type: "string" } },
          { name: "categoryId", in: "query", schema: { type: "string" } },
          {
            name: "taxonomy",
            in: "query",
            schema: { type: "string", enum: ["FOOD", "DRINK", "DESSERT", "OTHER"] },
          },
          { name: "status", in: "query", schema: { type: "string" } },
        ],
        responses: { "200": { description: "Merchant products" } },
      },
    },
    "/api/catalog/products/batch/availability": {
      patch: {
        tags: ["Products"],
        summary: "Batch update product availability",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductBatchAvailabilityRequest" },
            },
          },
        },
        responses: { "200": { description: "Products updated" } },
      },
    },
    "/api/catalog/products/{id}": {
      get: {
        tags: ["Products"],
        summary: "Get product by id",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Product detail" } },
      },
      put: {
        tags: ["Products"],
        summary: "Update product",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Product updated" } },
      },
      patch: {
        tags: ["Products"],
        summary: "Patch product",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Product patched" } },
      },
      delete: {
        tags: ["Products"],
        summary: "Delete product",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Product deleted" } },
      },
    },
    "/api/catalog/products/{id}/detail": {
      get: {
        tags: ["Products"],
        summary: "Get product detail",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Product detail with options" } },
      },
    },
    "/api/catalog/products/{id}/availability": {
      patch: {
        tags: ["Products"],
        summary: "Update product availability",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductAvailabilityRequest" },
            },
          },
        },
        responses: { "200": { description: "Availability updated" } },
      },
    },
    "/api/catalog/products/{id}/featured": {
      patch: {
        tags: ["Products"],
        summary: "Update product featured flag",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductFeaturedRequest" },
            },
          },
        },
        responses: { "200": { description: "Featured updated" } },
      },
    },
    "/api/catalog/products/{id}/restore": {
      patch: {
        tags: ["Products"],
        summary: "Restore product",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Product restored" } },
      },
    },
    "/api/catalog/products/{id}/options": {
      post: {
        tags: ["Products"],
        summary: "Create product option",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductOptionRequest" },
            },
          },
        },
        responses: { "201": { description: "Product option created" } },
      },
    },
    "/api/catalog/products/options/{optionId}": {
      put: {
        tags: ["Products"],
        summary: "Update product option",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "optionId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ProductOptionRequest" },
            },
          },
        },
        responses: { "200": { description: "Product option updated" } },
      },
      delete: {
        tags: ["Products"],
        summary: "Delete product option",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "optionId", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Product option deleted" } },
      },
    },
    "/api/catalog/reviews": {
      get: {
        tags: ["Reviews"],
        summary: "List reviews",
        parameters: [
          { name: "page", in: "query", schema: { type: "integer", minimum: 1, default: 1 } },
          { name: "limit", in: "query", schema: { type: "integer", minimum: 1, default: 20 } },
          { name: "rating", in: "query", schema: { type: "integer", minimum: 1, maximum: 5 } },
        ],
        responses: { "200": { description: "Paginated review list" } },
      },
      post: {
        tags: ["Reviews"],
        summary: "Create review",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ReviewCreateRequest" },
            },
          },
        },
        responses: { "201": { description: "Review created" } },
      },
    },
    "/api/catalog/reviews/{id}": {
      get: {
        tags: ["Reviews"],
        summary: "Get review by id",
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Review detail" } },
      },
      put: {
        tags: ["Reviews"],
        summary: "Update review",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ReviewUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Review updated" } },
      },
      patch: {
        tags: ["Reviews"],
        summary: "Patch review",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ReviewUpdateRequest" },
            },
          },
        },
        responses: { "200": { description: "Review patched" } },
      },
      delete: {
        tags: ["Reviews"],
        summary: "Delete review",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Review deleted" } },
      },
    },
    "/api/catalog/reviews/{id}/reply": {
      patch: {
        tags: ["Reviews"],
        summary: "Reply to review",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/ReviewReplyRequest" },
            },
          },
        },
        responses: { "200": { description: "Reply saved" } },
      },
      delete: {
        tags: ["Reviews"],
        summary: "Delete review reply",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Reply deleted" } },
      },
    },
    "/api/catalog/reviews/{id}/restore": {
      patch: {
        tags: ["Reviews"],
        summary: "Restore review",
        security: [{ bearerAuth: [] }],
        parameters: [{ name: "id", in: "path", required: true, schema: { type: "string", format: "uuid" } }],
        responses: { "200": { description: "Review restored" } },
      },
    },
    "/api/catalog/reviews/product/{productId}": {
      get: {
        tags: ["Reviews"],
        summary: "Get reviews by product",
        parameters: [{ name: "productId", in: "path", required: true, schema: { type: "string" } }],
        responses: { "200": { description: "Product reviews" } },
      },
    },
    "/api/catalog/reviews/product/{productId}/summary": {
      get: {
        tags: ["Reviews"],
        summary: "Get product review summary",
        parameters: [{ name: "productId", in: "path", required: true, schema: { type: "string" } }],
        responses: { "200": { description: "Product review summary" } },
      },
    },
    "/api/catalog/reviews/user/{userId}": {
      get: {
        tags: ["Reviews"],
        summary: "Get reviews by user",
        parameters: [{ name: "userId", in: "path", required: true, schema: { type: "string" } }],
        responses: { "200": { description: "User reviews" } },
      },
    },
    "/api/catalog/reviews/merchant/{merchantId}": {
      get: {
        tags: ["Reviews"],
        summary: "Get reviews by merchant",
        parameters: [{ name: "merchantId", in: "path", required: true, schema: { type: "string" } }],
        responses: { "200": { description: "Merchant reviews" } },
      },
    },
    "/api/catalog/uploads": {
      post: {
        tags: ["Uploads"],
        summary: "Upload files",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "multipart/form-data": {
              schema: { $ref: "#/components/schemas/UploadRequest" },
            },
          },
        },
        responses: { "201": { description: "Files uploaded" } },
      },
      delete: {
        tags: ["Uploads"],
        summary: "Delete uploaded files",
        security: [{ bearerAuth: [] }],
        requestBody: {
          required: true,
          content: {
            "application/json": {
              schema: { $ref: "#/components/schemas/DeleteUploadRequest" },
            },
          },
        },
        responses: { "200": { description: "Files deleted" } },
      },
    },
  },
} as const;
