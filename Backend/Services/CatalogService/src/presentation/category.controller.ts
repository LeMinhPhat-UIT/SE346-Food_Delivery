import { type Request, type Response } from 'express';
import { CategoryService } from "../application/category.service";
import Send from '../util/response';

const categoryService = new CategoryService();

export class CategoryController {
    async getAllCategories(req: Request, res: Response) {
    try {
      const categories = await categoryService.getAllCategories();
      return Send.success(res, categories);
    } catch (error: any) {
      return Send.error(res, null, error.message);
    }
  }
}