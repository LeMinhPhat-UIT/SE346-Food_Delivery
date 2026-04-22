import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { ProductController } from "./product.controller";
import {
  createProductSchema,
  productIdSchema,
  updateProductSchema,
} from "./product.schema";

const router = Router();
const productController = new ProductController();

router.get("/", productController.getAllProducts);

router.get("/:id", validate(productIdSchema), productController.getProductById);

router.post(
  "/",
  validate(createProductSchema),
  productController.createProduct,
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
