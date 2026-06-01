import "dotenv/config";
import { z } from "zod";

const envSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: z.coerce.number().int().positive().default(8088),
  DATABASE_URL: z.string().min(1, "DATABASE_URL is required"),
  DIRECT_URL: z.string().optional(),
  JWT_SECRET: z.string().min(1, "JWT_SECRET is required"),
  JWT_ISSUER: z.string().optional(),
  JWT_AUDIENCE: z.string().optional(),
  RABBITMQ_URL: z.string().url("RABBITMQ_URL must be a valid URL"),
  RABBITMQ_EXCHANGE: z.string().min(1).default("notify-exchange"),
  USER_SERVICE_URL: z.string().url().default("http://user-service:8080/api"),
  ORDER_SERVICE_URL: z.string().url().default("http://order-service:8080/api/orders"),
  DELIVERY_SERVICE_URL: z.string().url().default("http://delivery-service:8080/api/deliveries"),
  WALLET_MERCHANT_COMMISSION_RATE: z.coerce.number().min(0).max(100).default(22),
  WALLET_SHIPPER_COMMISSION_RATE: z.coerce.number().min(0).max(100).default(32),
  REPORT_DB_SCHEMA: z.string().default("public"),
});

export const env = envSchema.parse(process.env);
