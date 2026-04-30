import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import {
  categoryIdSchema,
  createCategorySchema,
  listCategoriesSchema,
  restoreCategorySchema,
  updateCategoryStatusSchema,
  updateCategorySchema,
} from "./category.schema";
import { CategoryController } from "./category.controller";

const router = Router();
const categoryController = new CategoryController();

router.get("/", validate(listCategoriesSchema), categoryController.getAllCategories);
router.get("/tree", categoryController.getCategoryTree);
router.get("/root", categoryController.getRootCategories);

router.post(
  "/",
  validate(createCategorySchema),
  categoryController.createCategory
);

router.patch(
  "/:id/status",
  validate(updateCategoryStatusSchema),
  categoryController.updateCategoryStatus
);

router.patch(
  "/:id/restore",
  validate(restoreCategorySchema),
  categoryController.restoreCategory
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
