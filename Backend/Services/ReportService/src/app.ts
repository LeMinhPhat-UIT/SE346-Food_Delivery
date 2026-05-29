import cors from "cors";
import express from "express";
import swaggerUi from "swagger-ui-express";
import reportRoutes from "./modules/report/report.route";
import { errorMiddleware } from "./middlewares/error.middleware";
import { openApiSpec } from "./docs/openapi";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Report Service is running healthy!");
});

app.get("/openapi.json", (_req, res) => {
  res.status(200).json(openApiSpec);
});

app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(openApiSpec, { explorer: true }));

app.use("/api/reports", reportRoutes);

app.use(errorMiddleware);

export default app;
