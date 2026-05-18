import { Router } from "express";
import { authenticate } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { OrderController } from "./order.controller";
import { checkoutPreviewSchema } from "./order.schema";

const router = Router();
const orderController = new OrderController();

router.use(authenticate);

router.post(
  "/checkout/preview",
  validate(checkoutPreviewSchema),
  orderController.previewCheckout,
);

export default router;
