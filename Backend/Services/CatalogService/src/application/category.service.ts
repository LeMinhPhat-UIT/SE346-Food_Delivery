import { CategoryInput } from "./../domain/category.schema";
import { prisma } from "../infrastructure/prisma.client";

class CategoryError extends Error {
  constructor(
    message: string,
    public readonly statusCode: number,
  ) {
    super(message);
    this.name = "CategoryError";
  }
}

export class CategoryService {
  private readonly categorySelect = {
    id: true,
    name: true,
    description: true,
    iconUrl: true,
    parentId: true,
    sortOrder: true,
    isActive: true,
    createdAt: true,
    deletedAt: true,
    parent: {
      select: {
        id: true,
        name: true,
      },
    },
    children: {
      where: {
        deletedAt: null,
      },
      select: {
        id: true,
        name: true,
      },
      orderBy: {
        sortOrder: "asc" as const,
      },
    },
  };

  async getAllCategories() {
    return prisma.category.findMany({
      where: {
        deletedAt: null,
      },
      orderBy: [{ sortOrder: "asc" }, { createdAt: "desc" }],
      select: this.categorySelect,
    });
  }

  async getCategoryById(id: string) {
    this.validateCategoryId(id);

    const category = prisma.category.findUnique({
      where: {
        id,
        deletedAt: null,
      },
      select: this.categorySelect,
    });

    if (!category) {
      throw new CategoryError("Category not found", 404);
    }

    return category;
  }

  async createCategory(data: CategoryInput) {
    if (data.parentId) await this.checkParentExists(data.parentId);

    return prisma.category.create({
      data,
      select: this.categorySelect,
    });
  }

  async updateCategory(id: string, data: Partial<CategoryInput>) {
    this.validateCategoryId(id);

    const existing = await prisma.category.findUnique({
      where: {
        id,
        deletedAt: null,
      },
      select: {
        id: true,
      },
    });

    if (!existing) {
      throw new CategoryError("Category not found", 404);
    }

    if (data.parentId) {
      if (data.parentId === id)
        throw new CategoryError("Category cannot be its own parent", 400);
      await this.checkParentExists(data.parentId);
    }

    return prisma.category.update({
      where: { id },
      data,
      select: this.categorySelect,
    });
  }

  async deleteCategory(id: string) {
    this.validateCategoryId(id);

    const existing = await prisma.category.findUnique({
      where: {
        id,
        deletedAt: null,
      },
      select: {
        id: true,
      },
    });

    if (!existing) {
      throw new CategoryError("Category not found", 404);
    }

    const [childCount, productCount] = await Promise.all([
      prisma.category.count({
        where: {
          parentId: id,
          deletedAt: null,
        },
      }),
      prisma.product.count({
        where: {
          categoryId: id,
          deletedAt: null,
        },
      }),
    ]);

    if (childCount > 0) {
      throw new CategoryError(
        "Cannot delete category because it still has child categories",
        400,
      );
    }

    if (productCount > 0) {
      throw new CategoryError(
        "Cannot delete category because it is being used by products",
        400,
      );
    }

    return prisma.category.update({
      where: { id },
      data: {
        isActive: false,
        deletedAt: new Date(),
      },
      select: this.categorySelect,
    });
  }

  private validateCategoryId(id: string) {
    if (!id || typeof id !== "string") {
      throw new CategoryError("Category id is required", 400);
    }
  }

  private async checkParentExists(parentId: string) {
    const exists = await prisma.category.findFirst({
      where: { id: parentId, deletedAt: null },
    });
    if (!exists) throw new CategoryError("Parent category does not exist", 400);
  }
}
