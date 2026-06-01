import "dotenv/config";
import { z } from "zod";

const databaseUrl = process.env.DATABASE_URL ?? process.env.SUPABASE_DATABASE_URL;
const directUrl = process.env.DIRECT_URL ?? process.env.SUPABASE_DIRECT_URL;

const envSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: z.coerce.number().int().positive().default(8080),
  DATABASE_URL: z.preprocess(
    () => databaseUrl,
    z.string().min(1, "DATABASE_URL is required"),
  ),
  DIRECT_URL: z.preprocess(() => directUrl, z.string().optional()),
  JWT_SECRET: z.string().min(1, "JWT_SECRET is required"),
  JWT_ISSUER: z.string().optional(),
  JWT_AUDIENCE: z.string().optional(),
  RABBITMQ_URL: z.string().url().default("amqp://appuser:app_password@rabbitmq:5672"),
  RABBITMQ_EXCHANGE: z.string().min(1).default("notify-exchange"),
  USER_SERVICE_URL: z.string().url().default("http://user-service:8080/api"),
  ORDER_SERVICE_URL: z.string().url().default("http://order-service:8080/api/orders"),
  DELIVERY_SERVICE_URL: z.string().url().default("http://delivery-service:8080/api/deliveries"),
});

export const env = envSchema.parse(process.env);
