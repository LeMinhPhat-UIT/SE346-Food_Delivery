import express from "express";
import cors from "cors";
import { errorMiddleware } from "./middlewares/error.middleware";
import cartRoutes from "./modules/cart/cart.route";
import voucherRoutes from "./modules/voucher/voucher.route";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Order Service is running healthy!");
});

app.use("/api/orders/vouchers", voucherRoutes);
app.use("/api/orders/cart", cartRoutes);

app.use(errorMiddleware);

export default app;
