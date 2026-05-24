import "dotenv/config";
import { z } from "zod";

const envSchema = z.object({
  NODE_ENV: z
    .enum(["development", "test", "production"])
    .default("development"),
  PORT: z.coerce.number().int().positive().default(8084),
  DATABASE_URL: z.string().min(1, "DATABASE_URL is required"),
  DIRECT_URL: z.string().optional(),
  JWT_SECRET: z.string().min(1, "JWT_SECRET is required"),
  JWT_ISSUER: z.string().optional(),
  JWT_AUDIENCE: z.string().optional(),
  RABBITMQ_URL: z.string().url("RABBITMQ_URL must be a valid URL"),
  RABBITMQ_EXCHANGE: z.string().min(1).default("notify-exchange"),
  USER_SERVICE_URL: z.string().url("USER_SERVICE_URL must be a valid URL"),
  CATALOG_SERVICE_URL: z.string().url("CATALOG_SERVICE_URL must be a valid URL"),
  DELIVERY_SERVICE_URL: z.string().url("DELIVERY_SERVICE_URL must be a valid URL"),
  REDIS_HOST: z.string().min(1, "REDIS_HOST is required"),
  REDIS_PORT: z.coerce.number().int().positive().default(6379),
  REDIS_PASSWORD: z.string().optional(),
  CART_TTL_SECONDS: z.coerce.number().int().positive().default(60 * 60 * 24 * 7),
  OUTBOX_POLL_INTERVAL_MS: z.coerce.number().int().positive().default(5000),
  OUTBOX_BATCH_SIZE: z.coerce.number().int().positive().max(100).default(25),
  VNPAY_URL: z.string().url("VNPAY_URL must be a valid URL"),
  VNPAY_TMN_CODE: z.string().min(1, "VNPAY_TMN_CODE is required"),
  VNPAY_HASH_SECRET: z.string().min(1, "VNPAY_HASH_SECRET is required"),
  VNPAY_RETURN_URL: z
    .string()
    .url("VNPAY_RETURN_URL must be a valid URL")
    .default("http://localhost:8086/api/orders/payments/vnpay/return"),
  VNPAY_IPN_URL: z
    .string()
    .url("VNPAY_IPN_URL must be a valid URL")
    .default("http://localhost:8086/api/orders/payments/vnpay/ipn"),
  VNPAY_VERSION: z.string().default("2.1.0"),
  VNPAY_COMMAND: z.string().default("pay"),
  VNPAY_CURRENCY: z.string().default("VND"),
  VNPAY_LOCALE: z.string().default("vn"),
  VNPAY_ORDER_TYPE: z.string().default("180000"),
  VNPAY_EXPIRE_MINUTES: z.coerce.number().int().positive().default(15),
  VNPAY_BANK_CODE: z.string().optional(),
});

export const env = envSchema.parse(process.env);
