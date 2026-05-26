import cors from "cors";
import express from "express";
import walletRoutes from "./modules/wallet/wallet.route";
import { errorMiddleware } from "./middlewares/error.middleware";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Wallet Service is running healthy!");
});

app.use("/api/wallets", walletRoutes);

app.use(errorMiddleware);

export default app;
