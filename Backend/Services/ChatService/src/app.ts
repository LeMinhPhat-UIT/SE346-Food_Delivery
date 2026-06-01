import express from "express";
import cors from "cors";
import swaggerUi from "swagger-ui-express";
import { errorMiddleware } from "./middlewares/error.middleware";
import chatRoutes from "./modules/chat/chat.route";
import { openApiSpec } from "./docs/openapi";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Chat Service is running healthy!");
});

app.get("/openapi.json", (_req, res) => {
  res.status(200).json(openApiSpec);
});

app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(openApiSpec, { explorer: true }));

app.use("/api/chats", chatRoutes);

app.use(errorMiddleware);

export default app;
