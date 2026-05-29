import express from "express";
import cors from "cors";
import swaggerUi from "swagger-ui-express";
import { errorMiddleware } from "./middlewares/error.middleware";
import cartRoutes from "./modules/cart/cart.route";
import orderRoutes from "./modules/order/order.route";
import paymentRoutes from "./modules/payment/payment.route";
import voucherRoutes from "./modules/voucher/voucher.route";
import { openApiSpec } from "./docs/openapi";

const app = express();

app.use(cors());
app.use(express.json());

app.get("/health", (_req, res) => {
  res.status(200).send("Order Service is running healthy!");
});

app.get("/openapi.json", (_req, res) => {
  res.status(200).json(openApiSpec);
});

app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(openApiSpec, { explorer: true }));

app.use("/api/orders/vouchers", voucherRoutes);
app.use("/api/orders/cart", cartRoutes);
app.use("/api/orders/payments", paymentRoutes);
app.use("/api/orders", orderRoutes);

app.use(errorMiddleware);

export default app;
