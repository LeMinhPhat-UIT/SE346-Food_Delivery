import express from "express";
import cors from "cors";
import categoryRouter from "./modules/category/category.route";
import { errorMiddleware } from "./middlewares/error.middleware";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Catalog Service is running healthy!");
});

app.use("/api/catalog/categories", categoryRouter);

app.use(errorMiddleware);

export default app;
