import { Router } from "express";
import { authenticate } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { paymentController } from "./payment.bootstrap";
import {
  createVnpayPaymentUrlBodySchema,
  orderIdParamSchema,
} from "./payment.schema";

const router = Router();

router.get("/vnpay/return", paymentController.handleVnpayReturn);
router.get("/vnpay/ipn", paymentController.handleVnpayIpn);

router.use(authenticate);

router.get(
  "/:orderId",
  validate({ params: orderIdParamSchema }),
  paymentController.getPaymentByOrderId,
);

router.post(
  "/:orderId/vnpay/url",
  validate({ params: orderIdParamSchema, body: createVnpayPaymentUrlBodySchema }),
  paymentController.createVnpayPaymentUrl,
);

export default router;

