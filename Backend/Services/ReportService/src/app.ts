import cors from "cors";
import express from "express";
import reportRoutes from "./modules/report/report.route";
import { errorMiddleware } from "./middlewares/error.middleware";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Report Service is running healthy!");
});

app.use("/api/reports", reportRoutes);

app.use(errorMiddleware);

export default app;
