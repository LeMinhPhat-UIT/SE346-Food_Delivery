const timestamp = () => new Date().toISOString();

export const logger = {
  info: (message: string, ...args: unknown[]) => console.log(`[INFO] ${timestamp()} ${message}`, ...args),
  warn: (message: string, ...args: unknown[]) => console.warn(`[WARN] ${timestamp()} ${message}`, ...args),
  error: (message: string, ...args: unknown[]) => console.error(`[ERROR] ${timestamp()} ${message}`, ...args),
};
