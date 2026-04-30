import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { UploadService } from "../upload/upload.service";
import {
  CategoryListResponseDto,
  CategoryQueryDto,
  CategoryResponseDto,
  CategoryTreeNodeDto,
  CreateCategoryDto,
  UpdateCategoryDto,
  UpdateCategoryStatusDto,
} from "./category.dto";
import { toCategoryResponseDto } from "./category.mapper";
import { CategoryRepository } from "./category.repository";

export class CategoryService {
  private readonly uploadService = new UploadService();

  constructor(private readonly categoryRepository: CategoryRepository) {}

  async getAllCategories(
    filters: CategoryQueryDto
  ): Promise<CategoryListResponseDto> {
    const { items, totalCount } = await this.categoryRepository.findAll(filters);

    return {
      items: items.map(toCategoryResponseDto),
      totalCount,
      page: filters.page,
      limit: filters.limit,
      totalPages: Math.ceil(totalCount / filters.limit),
    };
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
    const existingCategory = await this.ensureCategoryExists(id);

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

    if (
      data.iconUrl !== undefined &&
      existingCategory.iconUrl &&
      data.iconUrl !== existingCategory.iconUrl
    ) {
      await this.uploadService.deleteFilesByPublicUrls([existingCategory.iconUrl]);
    }

    return toCategoryResponseDto(category);
  }

  async getRootCategories(): Promise<CategoryResponseDto[]> {
    const categories = await this.categoryRepository.findRoots();
    return categories.map(toCategoryResponseDto);
  }

  async getCategoryTree(): Promise<CategoryTreeNodeDto[]> {
    const categories = await this.categoryRepository.findTreeCategories();
    const categoryMap = new Map<string, CategoryTreeNodeDto>();
    const roots: CategoryTreeNodeDto[] = [];

    for (const category of categories) {
      categoryMap.set(category.id, {
        ...toCategoryResponseDto(category),
        children: [],
      });
    }

    for (const category of categories) {
      const currentNode = categoryMap.get(category.id)!;

      if (category.parentId) {
        const parentNode = categoryMap.get(category.parentId);

        if (parentNode) {
          parentNode.children.push(currentNode);
          continue;
        }
      }

      roots.push(currentNode);
    }

    return roots;
  }

  async updateCategoryStatus(
    id: string,
    data: UpdateCategoryStatusDto
  ): Promise<CategoryResponseDto> {
    const category = await this.ensureCategoryExists(id);

    if (data.isActive && category.parentId) {
      await this.ensureParentExists(category.parentId);
    }

    const updatedCategory = await this.categoryRepository.update(id, {
      isActive: data.isActive,
    });

    return toCategoryResponseDto(updatedCategory);
  }

  async restoreCategory(id: string): Promise<CategoryResponseDto> {
    const category = await this.categoryRepository.findByIdIncludingDeleted(id);

    if (!category) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Category not found");
    }

    if (category.deletedAt === null) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Category is already active"
      );
    }

    if (category.parentId) {
      const parent = await this.categoryRepository.findById(category.parentId);

      if (!parent || !parent.isActive) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Cannot restore category because parent category is missing or inactive"
        );
      }
    }

    const restoredCategory = await this.categoryRepository.update(id, {
      deletedAt: null,
      isActive: true,
    });

    return toCategoryResponseDto(restoredCategory);
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
