const format = (level: string, message: string, meta?: unknown) => {
  if (meta === undefined) {
    return `[${level}] ${message}`;
  }

  return `[${level}] ${message} ${JSON.stringify(meta)}`;
};

export const logger = {
  info: (message: string, meta?: unknown) => console.log(format("INFO", message, meta)),
  warn: (message: string, meta?: unknown) => console.warn(format("WARN", message, meta)),
  error: (message: string, meta?: unknown) => console.error(format("ERROR", message, meta)),
  debug: (message: string, meta?: unknown) => console.debug(format("DEBUG", message, meta)),
};
