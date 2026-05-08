import { ROLES } from "../../constants/roles";
import { authenticate, requireRoles } from "../../middlewares/auth.middleware";
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
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(createCategorySchema),
  categoryController.createCategory
);

router.patch(
  "/:id/status",
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(updateCategoryStatusSchema),
  categoryController.updateCategoryStatus
);

router.patch(
  "/:id/restore",
  authenticate,
  requireRoles(ROLES.ADMIN),
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
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(updateCategorySchema),
  categoryController.updateCategory
);

router.patch(
  "/:id",
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(updateCategorySchema),
  categoryController.updateCategory
);

router.delete(
  "/:id",
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(categoryIdSchema),
  categoryController.deleteCategory
);

export default router;
