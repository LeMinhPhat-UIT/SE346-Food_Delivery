declare namespace Express {
  interface Request {
    validated?: {
      body?: unknown;
      params?: unknown;
      query?: unknown;
    };
  }
}
