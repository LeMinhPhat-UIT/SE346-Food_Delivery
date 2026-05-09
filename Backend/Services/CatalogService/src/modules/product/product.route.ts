import {
  attachMerchantContext,
  authenticate,
  requireRoles,
} from "../../middlewares/auth.middleware";
import { ROLES } from "../../constants/roles";
import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { ProductController } from "./product.controller";
import {
  batchUpdateProductAvailabilitySchema,
  createProductSchema,
  createProductOptionSchema,
  listProductsSchema,
  merchantProductsSchema,
  productOptionIdSchema,
  productIdSchema,
  restoreProductSchema,
  updateProductAvailabilitySchema,
  updateProductFeaturedSchema,
  updateProductOptionSchema,
  updateProductSchema,
} from "./product.schema";

const router = Router();
const productController = new ProductController();

router.get("/", validate(listProductsSchema), productController.getAllProducts);

router.get(
  "/merchant/me",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(merchantProductsSchema),
  productController.getMyProducts,
);

router.patch(
  "/batch/availability",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(batchUpdateProductAvailabilitySchema),
  productController.batchUpdateProductAvailability,
);

router.get("/:id/detail", validate(productIdSchema), productController.getProductDetail);

router.get("/:id", validate(productIdSchema), productController.getProductById);

router.post(
  "/",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(createProductSchema),
  productController.createProduct,
);

router.post(
  "/:id/options",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(createProductOptionSchema),
  productController.createProductOption,
);

router.put(
  "/options/:optionId",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateProductOptionSchema),
  productController.updateProductOption,
);

router.delete(
  "/options/:optionId",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(productOptionIdSchema),
  productController.deleteProductOption,
);

router.patch(
  "/:id/availability",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateProductAvailabilitySchema),
  productController.updateProductAvailability,
);

router.patch(
  "/:id/featured",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateProductFeaturedSchema),
  productController.updateProductFeatured,
);

router.patch(
  "/:id/restore",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(restoreProductSchema),
  productController.restoreProduct,
);

router.put(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateProductSchema),
  productController.updateProduct,
);

router.patch(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateProductSchema),
  productController.updateProduct,
);

router.delete(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(productIdSchema),
  productController.deleteProduct,
);

export default router;
