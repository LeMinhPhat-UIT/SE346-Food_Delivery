declare namespace Express {
  interface AuthUser {
    userId: string;
    email?: string;
    roles: string[];
    merchantId?: string;
    token?: string;
    claims: Record<string, unknown>;
  }

  interface Request {
    validated?: {
      body?: unknown;
      params?: unknown;
      query?: unknown;
    };
    auth?: AuthUser;
  }
}
