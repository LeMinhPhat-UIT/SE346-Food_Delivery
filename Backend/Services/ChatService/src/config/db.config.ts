import { env } from "./env.config";

export const dbConfig = {
  databaseUrl: env.DATABASE_URL,
  directUrl: env.DIRECT_URL,
};
