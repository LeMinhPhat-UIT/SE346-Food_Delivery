import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import {
  CategoryResponseDto,
  CreateCategoryDto,
  UpdateCategoryDto,
} from "./category.dto";
import { toCategoryResponseDto } from "./category.mapper";
import { CategoryRepository } from "./category.repository";

export class CategoryService {
  constructor(private readonly categoryRepository: CategoryRepository) {}

  async getAllCategories(): Promise<CategoryResponseDto[]> {
    const categories = await this.categoryRepository.findAll();
    return categories.map(toCategoryResponseDto);
  }

  async getCategoryById(id: string): Promise<CategoryResponseDto> {
    const category = await this.categoryRepository.findById(id);

    if (!category) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Category not found");
    }

    return toCategoryResponseDto(category);
  }

  async createCategory(data: CreateCategoryDto): Promise<CategoryResponseDto> {
    const existing = await this.categoryRepository.findByName(data.name);

    if (existing) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Category name already exists",
      );
    }

    if (data.parentId) {
      await this.ensureParentExists(data.parentId);
    }

    const category = await this.categoryRepository.create(data);
    return toCategoryResponseDto(category);
  }

  async updateCategory(
    id: string,
    data: UpdateCategoryDto,
  ): Promise<CategoryResponseDto> {
    await this.ensureCategoryExists(id);

    if (data.parentId !== undefined) {
      if (data.parentId === id) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Category cannot be its own parent",
        );
      }

      if (data.parentId) {
        await this.ensureParentExists(data.parentId);
        await this.ensureNoCycle(id, data.parentId);
      }
    }

    const category = await this.categoryRepository.update(id, data);
    return toCategoryResponseDto(category);
  }

  async deleteCategory(id: string): Promise<CategoryResponseDto> {
    await this.ensureCategoryExists(id);

    const [childCount, productCount] = await Promise.all([
      this.categoryRepository.countActiveChildren(id),
      this.categoryRepository.countProductsUsingCategory(id),
    ]);

    if (childCount > 0) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Cannot delete category because it still has child categories",
      );
    }

    if (productCount > 0) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Cannot delete category because it is being used by products",
      );
    }

    const category = await this.categoryRepository.update(id, {
      isActive: false,
      deletedAt: new Date(),
    });

    return toCategoryResponseDto(category);
  }

  private async ensureCategoryExists(id: string) {
    const category = await this.categoryRepository.findById(id);

    if (!category) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Category not found");
    }

    return category;
  }

  private async ensureParentExists(parentId: string) {
    const parent = await this.categoryRepository.findById(parentId);

    if (!parent || !parent.isActive) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Parent category does not exist or is inactive",
      );
    }
  }

  private async ensureNoCycle(id: string, parentId: string) {
    let current: string | null = parentId;

    while (current) {
      if (current === id) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Cycle detected in category hierarchy",
        );
      }

      const parent = await this.categoryRepository.findById(current);
      current = parent?.parentId ?? null;
    }
  }
}
