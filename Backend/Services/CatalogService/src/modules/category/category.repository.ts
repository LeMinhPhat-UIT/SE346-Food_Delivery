import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { CreateCategoryDto } from "./category.dto";

export const categorySelect = Prisma.validator<Prisma.CategorySelect>()({
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
      sortOrder: "asc",
    },
  },
});

export type CategoryRecord = Prisma.CategoryGetPayload<{
  select: typeof categorySelect;
}>;

export class CategoryRepository {
  async findAll() {
    return prisma.category.findMany({
      where: {
        deletedAt: null,
      },
      orderBy: [{ sortOrder: "asc" }, { createdAt: "desc" }],
      select: categorySelect,
    });
  }

  async findById(id: string) {
    const category = await prisma.category.findUnique({
      where: { id },
      select: categorySelect,
    });

    if (!category || category.deletedAt !== null) return null;

    return category;
  }

  async findByName(name: string) {
    return prisma.category.findFirst({
      where: {
        name,
        deletedAt: null,
      },
      select: categorySelect,
    });
  }

  async create(data: CreateCategoryDto) {
    return prisma.category.create({
      data,
      select: categorySelect,
    });
  }

  async update(id: string, data: Prisma.CategoryUpdateInput) {
    return prisma.category.update({
      where: { id },
      data,
      select: categorySelect,
    });
  }

  async countActiveChildren(id: string) {
    return prisma.category.count({
      where: {
        parentId: id,
        deletedAt: null,
      },
    });
  }

  async countProductsUsingCategory(id: string) {
    return prisma.product.count({
      where: {
        categoryId: id,
        deletedAt: null,
      },
    });
  }
}
