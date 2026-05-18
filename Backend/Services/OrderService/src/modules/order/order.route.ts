import { Router } from "express";
import {
  attachMerchantContext,
  authenticate,
  requireRoles,
} from "../../middlewares/auth.middleware";
import { ROLES } from "../../constants/roles";
import { validate } from "../../middlewares/validate.middleware";
import { OrderController } from "./order.controller";
import {
  cancelOrderSchema,
  checkoutPreviewSchema,
  createOrderSchema,
  myOrdersSchema,
  orderIdSchema,
  updateOrderStatusSchema,
} from "./order.schema";

const router = Router();
const orderController = new OrderController();

router.use(authenticate);

router.get(
  "/my",
  validate(myOrdersSchema),
  orderController.getMyOrders,
);
router.get(
  "/merchant/my",
  requireRoles(ROLES.MERCHANT),
  attachMerchantContext,
  validate(myOrdersSchema),
  orderController.getMerchantOrders,
);
router.get(
  "/merchant/my/:id",
  requireRoles(ROLES.MERCHANT),
  attachMerchantContext,
  validate(orderIdSchema),
  orderController.getMerchantOrderById,
);
router.patch(
  "/merchant/my/:id/status",
  requireRoles(ROLES.MERCHANT),
  attachMerchantContext,
  validate(updateOrderStatusSchema),
  orderController.updateMerchantOrderStatus,
);
router.get(
  "/:id",
  validate(orderIdSchema),
  orderController.getOrderById,
);
router.patch(
  "/:id/cancel",
  validate(cancelOrderSchema),
  orderController.cancelMyOrder,
);
router.post(
  "/checkout/preview",
  validate(checkoutPreviewSchema),
  orderController.previewCheckout,
);
router.post(
  "/",
  validate(createOrderSchema),
  orderController.createOrder,
);

export default router;
