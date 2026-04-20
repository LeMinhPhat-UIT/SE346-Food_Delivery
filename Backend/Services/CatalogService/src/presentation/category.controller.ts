import { type Request, type Response } from "express";
import { CategoryService } from "../application/category.service";
import Send from "../util/response";
import { CategorySchema, UpdateCategorySchema } from "../domain/category.schema";
import { ZodError } from "zod";

const categoryService = new CategoryService();

export class CategoryController {
  private getCategoryId(req: Request) {
    const { id } = req.params;

    return Array.isArray(id) ? id[0] : id;
  }

  private handleError(res: Response, error: unknown) {
    if (error instanceof ZodError) {
      const errorMessage = error.issues.map(err => err.message).join(", ");
      return Send.badRequest(res, null, errorMessage);
    }

    const message = error instanceof Error ? error.message : "Internal server error";
    const statusCode =
      typeof error === "object" &&
      error !== null &&
      "statusCode" in error &&
      typeof error.statusCode === "number"
        ? error.statusCode
        : 500;

    if (statusCode === 404) return Send.notFound(res, null, message);
    if (statusCode === 400) return Send.badRequest(res, null, message);

    return Send.error(res, null, message);
  }

  getAllCategories = async (req: Request, res: Response) => {
    try {
      const categories = await categoryService.getAllCategories();
      return Send.success(res, categories);
    } 
    catch (error) {
      return this.handleError(res, error);
    }
  };

  getCategoryById = async (req: Request, res: Response) => {
    try {
      const category = await categoryService.getCategoryById(this.getCategoryId(req));
      return Send.success(res, category);
    } 
    catch (error) {
      return this.handleError(res, error);
    }
  };

  createCategory = async (req: Request, res: Response) => {
    try {
      const validatedData = CategorySchema.parse(req.body);
      const category = await categoryService.createCategory(validatedData);
      return Send.created(res, category, "Category created successfully");
    } 
    catch (error) {
      return this.handleError(res, error);
    }
  };

  updateCategory = async (req: Request, res: Response) => {
    try {
      const validatedData = UpdateCategorySchema.parse(req.body);
      const category = await categoryService.updateCategory(
        this.getCategoryId(req),
        validatedData
      );
      return Send.success(res, category, "Category updated successfully");
    } 
    catch (error) {
      return this.handleError(res, error);
    }
  };

  deleteCategory = async (req: Request, res: Response) => {
    try {
      const category = await categoryService.deleteCategory(this.getCategoryId(req));
      return Send.success(res, category, "Category deleted successfully");
    } 
    catch (error) {
      return this.handleError(res, error);
    }
  };
}
