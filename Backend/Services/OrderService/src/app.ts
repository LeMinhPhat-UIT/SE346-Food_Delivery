import express from "express";
import cors from "cors";
import { errorMiddleware } from "./middlewares/error.middleware";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Order Service is running healthy!");
});

app.use(errorMiddleware);

export default app;
