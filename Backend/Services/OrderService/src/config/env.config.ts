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
  USER_SERVICE_URL: z.string().url("USER_SERVICE_URL must be a valid URL"),
  CATALOG_SERVICE_URL: z.string().url("CATALOG_SERVICE_URL must be a valid URL"),
  REDIS_HOST: z.string().min(1, "REDIS_HOST is required"),
  REDIS_PORT: z.coerce.number().int().positive().default(6379),
  REDIS_PASSWORD: z.string().optional(),
  CART_TTL_SECONDS: z.coerce.number().int().positive().default(60 * 60 * 24 * 7),
});

export const env = envSchema.parse(process.env);
