declare module "swagger-ui-express" {
  import { RequestHandler } from "express";

  const swaggerUi: {
    serve: RequestHandler;
    setup(swaggerDoc: unknown, options?: Record<string, unknown>): RequestHandler;
  };

  export default swaggerUi;
}
