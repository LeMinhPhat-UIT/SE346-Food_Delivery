import "express-serve-static-core";

declare module "express-serve-static-core" {
  interface Request {
    auth?: {
      userId: string;
      email?: string;
      roles: string[];
      token: string;
      claims: Record<string, unknown>;
      merchantId?: string;
      shipperId?: string;
    };
  }
}
