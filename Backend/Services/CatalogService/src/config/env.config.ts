import "dotenv/config";
import { z } from "zod";

const envSchema = z.object({
  NODE_ENV: z
    .enum(["development", "test", "production"])
    .default("development"),
  PORT: z.coerce.number().int().positive().default(8084),
  DATABASE_URL: z.string().min(1, "DATABASE_URL is required"),
  DIRECT_URL: z.string().optional(),
  JWT_SECRET: z.string().optional(),
  JWT_ISSUER: z.string().optional(),
  JWT_AUDIENCE: z.string().optional(),
  USER_SERVICE_URL: z.string().url().default("http://user-service:8080/api"),
  ORDER_SERVICE_URL: z.string().url().default("http://order-service:8080/api/orders"),
  SUPABASE_PROJECT_URL: z.string().url("SUPABASE_PROJECT_URL must be a valid URL"),
  SUPABASE_ANON_KEY: z.string().min(1, "SUPABASE_ANON_KEY is required"),
  SUPABASE_SERVICE_ROLE_KEY: z.string().optional(),
  SUPABASE_STORAGE_BUCKET: z.string().default("catalog-bucket"),
});

export const env = envSchema.parse(process.env);
