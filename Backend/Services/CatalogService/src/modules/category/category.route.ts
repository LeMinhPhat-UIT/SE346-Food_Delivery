import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import {
  categoryIdSchema,
  createCategorySchema,
  updateCategorySchema,
} from "./category.schema";
import { CategoryController } from "./category.controller";

const router = Router();
const categoryController = new CategoryController();

router.get("/", categoryController.getAllCategories);

router.post(
  "/",
  validate(createCategorySchema),
  categoryController.createCategory
);

router.get(
  "/:id",
  validate(categoryIdSchema),
  categoryController.getCategoryById
);

router.put(
  "/:id",
  validate(updateCategorySchema),
  categoryController.updateCategory
);

router.patch(
  "/:id",
  validate(updateCategorySchema),
  categoryController.updateCategory
);

router.delete(
  "/:id",
  validate(categoryIdSchema),
  categoryController.deleteCategory
);

export default router;
