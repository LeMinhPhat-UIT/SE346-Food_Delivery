import { Router } from "express";
import { authenticate } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { CartController } from "./cart.controller";
import {
  addCartItemSchema,
  cartItemIdSchema,
  merchantCartSchema,
  updateCartItemSchema,
} from "./cart.schema";

const router = Router();
const cartController = new CartController();

router.use(authenticate);

router.get("/", cartController.getMyCarts);
router.get(
  "/merchant/:merchantId",
  validate(merchantCartSchema),
  cartController.getCartByMerchant,
);
router.post("/items", validate(addCartItemSchema), cartController.addCartItem);
router.patch(
  "/items/:itemId",
  validate(updateCartItemSchema),
  cartController.updateCartItem,
);
router.delete(
  "/items/:itemId",
  validate(cartItemIdSchema),
  cartController.removeCartItem,
);
router.delete(
  "/merchant/:merchantId",
  validate(merchantCartSchema),
  cartController.clearCartByMerchant,
);
router.delete("/", cartController.clearCart);

export default router;
