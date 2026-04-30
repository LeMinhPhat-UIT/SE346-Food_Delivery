import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import {
  CategoryQueryDto,
  CreateCategoryDto,
  UpdateCategoryDto,
  UpdateCategoryStatusDto,
} from "./category.dto";
import { CategoryRepository } from "./category.repository";
import { CategoryService } from "./category.service";

const categoryRepository = new CategoryRepository();
const categoryService = new CategoryService(categoryRepository);

export class CategoryController {
  getAllCategories = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as CategoryQueryDto;
    const categories = await categoryService.getAllCategories(filters);
    return Send.success(res, categories, "Categories fetched successfully");
  });

  getRootCategories = asyncHandler(async (_req: Request, res: Response) => {
    const categories = await categoryService.getRootCategories();
    return Send.success(res, categories, "Root categories fetched successfully");
  });

  getCategoryTree = asyncHandler(async (_req: Request, res: Response) => {
    const categories = await categoryService.getCategoryTree();
    return Send.success(res, categories, "Category tree fetched successfully");
  });

  getCategoryById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const category = await categoryService.getCategoryById(id);

    return Send.success(res, category, "Category fetched successfully");
  });

  createCategory = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as CreateCategoryDto;
    const category = await categoryService.createCategory(payload);

    return Send.success(
      res,
      category,
      "Category created successfully",
      HTTP_STATUS.CREATED
    );
  });

  updateCategory = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateCategoryDto;
    const category = await categoryService.updateCategory(id, payload);

    return Send.success(res, category, "Category updated successfully");
  });

  updateCategoryStatus = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateCategoryStatusDto;
    const category = await categoryService.updateCategoryStatus(id, payload);

    return Send.success(res, category, "Category status updated successfully");
  });

  restoreCategory = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const category = await categoryService.restoreCategory(id);

    return Send.success(res, category, "Category restored successfully");
  });

  deleteCategory = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const category = await categoryService.deleteCategory(id);

    return Send.success(res, category, "Category deleted successfully");
  });
}
