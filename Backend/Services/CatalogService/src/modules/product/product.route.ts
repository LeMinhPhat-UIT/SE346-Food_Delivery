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

router.get("/merchant/me", validate(merchantProductsSchema), productController.getMyProducts);

router.patch(
  "/batch/availability",
  validate(batchUpdateProductAvailabilitySchema),
  productController.batchUpdateProductAvailability,
);

router.get("/:id/detail", validate(productIdSchema), productController.getProductDetail);

router.get("/:id", validate(productIdSchema), productController.getProductById);

router.post(
  "/",
  validate(createProductSchema),
  productController.createProduct,
);

router.post(
  "/:id/options",
  validate(createProductOptionSchema),
  productController.createProductOption,
);

router.put(
  "/options/:optionId",
  validate(updateProductOptionSchema),
  productController.updateProductOption,
);

router.delete(
  "/options/:optionId",
  validate(productOptionIdSchema),
  productController.deleteProductOption,
);

router.patch(
  "/:id/availability",
  validate(updateProductAvailabilitySchema),
  productController.updateProductAvailability,
);

router.patch(
  "/:id/featured",
  validate(updateProductFeaturedSchema),
  productController.updateProductFeatured,
);

router.patch(
  "/:id/restore",
  validate(restoreProductSchema),
  productController.restoreProduct,
);

router.put(
  "/:id",
  validate(updateProductSchema),
  productController.updateProduct,
);

router.patch(
  "/:id",
  validate(updateProductSchema),
  productController.updateProduct,
);

router.delete(
  "/:id",
  validate(productIdSchema),
  productController.deleteProduct,
);

export default router;
