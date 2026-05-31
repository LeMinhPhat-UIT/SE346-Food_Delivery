import "dotenv/config";
import { z } from "zod";

const envSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: z.coerce.number().int().positive().default(8089),
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
  VNPAY_URL: z.string().url("VNPAY_URL must be a valid URL"),
  VNPAY_TMN_CODE: z.string().min(1, "VNPAY_TMN_CODE is required"),
  VNPAY_HASH_SECRET: z.string().min(1, "VNPAY_HASH_SECRET is required"),
  WALLET_VNPAY_RETURN_URL: z
    .string()
    .url("WALLET_VNPAY_RETURN_URL must be a valid URL")
    .default("http://localhost:8089/api/wallets/topup/vnpay/return"),
  WALLET_VNPAY_IPN_URL: z
    .string()
    .url("WALLET_VNPAY_IPN_URL must be a valid URL")
    .default("http://localhost:8089/api/wallets/topup/vnpay/ipn"),
  VNPAY_VERSION: z.string().default("2.1.0"),
  VNPAY_COMMAND: z.string().default("pay"),
  VNPAY_CURRENCY: z.string().default("VND"),
  VNPAY_LOCALE: z.string().default("vn"),
  VNPAY_ORDER_TYPE: z.string().default("180000"),
  VNPAY_EXPIRE_MINUTES: z.coerce.number().int().positive().default(15),
  WALLET_PLATFORM_OWNER_ID: z.string().min(1).default("55555555-5555-5555-5555-555555555555"),
  WALLET_MERCHANT_COMMISSION_RATE: z.coerce.number().min(0).max(100).default(22),
  WALLET_SHIPPER_COMMISSION_RATE: z.coerce.number().min(0).max(100).default(32),
});

export const env = envSchema.parse(process.env);
