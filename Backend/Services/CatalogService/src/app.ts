import express from "express";
import cors from "cors";
import swaggerUi from "swagger-ui-express";
import categoryRouter from "./modules/category/category.route";
import productRouter from "./modules/product/product.route";
import reviewRouter from "./modules/review/review.route";
import uploadRouter from "./modules/upload/upload.route";
import { errorMiddleware } from "./middlewares/error.middleware";
import { openApiSpec } from "./docs/openapi";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Catalog Service is running healthy!");
});

app.get("/openapi.json", (_req, res) => {
  res.status(200).json(openApiSpec);
});

app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(openApiSpec, { explorer: true }));

app.use("/api/catalog/categories", categoryRouter);
app.use("/api/catalog/products", productRouter);
app.use("/api/catalog/reviews", reviewRouter);
app.use("/api/catalog/uploads", uploadRouter);

app.use(errorMiddleware);

export default app;
