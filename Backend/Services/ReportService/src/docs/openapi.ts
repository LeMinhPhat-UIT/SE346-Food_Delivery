const bearerAuth = {
  type: "http",
  scheme: "bearer",
  bearerFormat: "JWT",
  description: "Paste access token returned from Authentication Service login.",
};

export const openApiSpec = {
  openapi: "3.0.3",
  info: {
    title: "Report Service API",
    version: "1.0.0",
    description: "Swagger documentation for Report Service endpoints.",
  },
  servers: [
    { url: "http://localhost:8088", description: "Docker host port" },
    { url: "http://localhost:8080", description: "Local development port" },
  ],
  tags: [
    { name: "Health", description: "Service health check" },
    { name: "Admin", description: "Admin reports and ranking dashboards" },
    { name: "Merchant", description: "Merchant revenue dashboards" },
    { name: "Shipper", description: "Shipper performance dashboards" },
  ],
  components: {
    securitySchemes: {
      bearerAuth,
    },
    schemas: {
      DateRangeQuery: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time", example: "2026-05-01T00:00:00.000Z" },
          to: { type: "string", format: "date-time", example: "2026-05-28T23:59:59.000Z" },
        },
      },
      AdminOverviewResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          summary: { type: "object", additionalProperties: { type: "number" } },
          daily: { type: "array", items: { type: "object" } },
        },
      },
      MerchantOverviewResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          summary: { type: "object", additionalProperties: { type: "number" } },
          daily: { type: "array", items: { type: "object" } },
        },
      },
      ShipperOverviewResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          summary: { type: "object", additionalProperties: { type: "number" } },
          daily: { type: "array", items: { type: "object" } },
        },
      },
      TopMerchantsResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          items: { type: "array", items: { type: "object" } },
        },
      },
      TopShippersResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          items: { type: "array", items: { type: "object" } },
        },
      },
      TopProductsResponse: {
        type: "object",
        properties: {
          from: { type: "string", format: "date-time" },
          to: { type: "string", format: "date-time" },
          items: {
            type: "array",
            items: {
              type: "object",
              properties: {
                productId: { type: "string", format: "uuid" },
                productName: { type: "string" },
                productImage: { type: "string", nullable: true },
                quantitySold: { type: "number" },
                orderCount: { type: "number" },
              },
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
          "200": {
            description: "Service is healthy",
            content: {
              "text/plain": {
                schema: { type: "string", example: "Report Service is running healthy!" },
              },
            },
          },
        },
      },
    },
    "/api/reports/admin/overview": {
      get: {
        tags: ["Admin"],
        summary: "Get admin overview",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Admin overview", content: { "application/json": { schema: { $ref: "#/components/schemas/AdminOverviewResponse" } } } } },
      },
    },
    "/api/reports/admin/top-merchants": {
      get: {
        tags: ["Admin"],
        summary: "Get top merchants",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Top merchants", content: { "application/json": { schema: { $ref: "#/components/schemas/TopMerchantsResponse" } } } } },
      },
    },
    "/api/reports/admin/top-shippers": {
      get: {
        tags: ["Admin"],
        summary: "Get top shippers",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Top shippers", content: { "application/json": { schema: { $ref: "#/components/schemas/TopShippersResponse" } } } } },
      },
    },
    "/api/reports/admin/top-products": {
      get: {
        tags: ["Admin"],
        summary: "Get top products",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Top products", content: { "application/json": { schema: { $ref: "#/components/schemas/TopProductsResponse" } } } } },
      },
    },
    "/api/reports/merchant/me/overview": {
      get: {
        tags: ["Merchant"],
        summary: "Get merchant overview",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Merchant overview", content: { "application/json": { schema: { $ref: "#/components/schemas/MerchantOverviewResponse" } } } } },
      },
    },
    "/api/reports/merchant/me/top-products": {
      get: {
        tags: ["Merchant"],
        summary: "Get merchant top products",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Merchant top products", content: { "application/json": { schema: { $ref: "#/components/schemas/TopProductsResponse" } } } } },
      },
    },
    "/api/reports/shipper/me/overview": {
      get: {
        tags: ["Shipper"],
        summary: "Get shipper overview",
        security: [{ bearerAuth: [] }],
        parameters: [
          { name: "from", in: "query", schema: { type: "string", format: "date-time" } },
          { name: "to", in: "query", schema: { type: "string", format: "date-time" } },
        ],
        responses: { "200": { description: "Shipper overview", content: { "application/json": { schema: { $ref: "#/components/schemas/ShipperOverviewResponse" } } } } },
      },
    },
  },
} as const;
