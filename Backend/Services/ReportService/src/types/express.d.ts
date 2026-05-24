import "express";

declare module "express-serve-static-core" {
  interface Request {
    auth?: {
      userId: string;
      email?: string;
      roles: string[];
      token: string;
      claims?: Record<string, unknown>;
      merchantId?: string;
      shipperId?: string;
    };
    validated?: {
      body?: unknown;
      params?: unknown;
      query?: unknown;
    };
  }
}

export {};
