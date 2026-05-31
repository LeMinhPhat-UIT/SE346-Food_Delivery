import app from "./app";
import { env } from "./config/env.config";
import { logger } from "./utils/logger";

const port = env.PORT;

app.listen(port, () => {
  logger.info(`Chat Service listening on port ${port}`);
});
