import { Router } from "express";
import { CategoryController } from "./category.controller";

const router = Router();
const categoryController = new CategoryController();

router.get("/all", categoryController.getAllCategories);
router.get("/:id", categoryController.getCategoryById);
router.post("/create", categoryController.createCategory);
router.put("/update/:id", categoryController.updateCategory);
router.patch("/update/:id", categoryController.updateCategory);
router.delete("/delete/:id", categoryController.deleteCategory);

export default router;
