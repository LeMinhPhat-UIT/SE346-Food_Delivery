import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { CategoryQueryDto, CreateCategoryDto } from "./category.dto";

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
  _count: {
    select: {
      products: {
        where: {
          deletedAt: null,
        },
      },
    },
  },
});

export type CategoryRecord = Prisma.CategoryGetPayload<{
  select: typeof categorySelect;
}>;

export class CategoryRepository {
  async findAll(filters: CategoryQueryDto) {
    const {
      page,
      limit,
      search,
      parentId,
      isActive,
      includeDeleted,
      sortBy,
      sortOrder,
    } = filters;

    const where: Prisma.CategoryWhereInput = {
      deletedAt: includeDeleted ? undefined : null,
      parentId,
      isActive,
      ...(search
        ? {
            OR: [
              {
                name: {
                  contains: search,
                  mode: "insensitive",
                },
              },
              {
                description: {
                  contains: search,
                  mode: "insensitive",
                },
              },
            ],
          }
        : {}),
    };

    const [items, totalCount] = await Promise.all([
      prisma.category.findMany({
        where,
        orderBy:
          sortBy === "productCount"
            ? {
                products: {
                  _count: sortOrder,
                },
              }
            : {
                [sortBy]: sortOrder,
              },
        skip: (page - 1) * limit,
        take: limit,
        select: categorySelect,
      }),
      prisma.category.count({ where }),
    ]);

    return { items, totalCount };
  }

  async findById(id: string) {
    const category = await prisma.category.findUnique({
      where: { id },
      select: categorySelect,
    });

    if (!category || category.deletedAt !== null) return null;

    return category;
  }

  async findByIdIncludingDeleted(id: string) {
    return prisma.category.findUnique({
      where: { id },
      select: categorySelect,
    });
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

  async findRoots() {
    return prisma.category.findMany({
      where: {
        parentId: null,
        deletedAt: null,
        isActive: true,
      },
      orderBy: [{ sortOrder: "asc" }, { name: "asc" }],
      select: categorySelect,
    });
  }

  async findTreeCategories() {
    return prisma.category.findMany({
      where: {
        deletedAt: null,
        isActive: true,
      },
      orderBy: [{ sortOrder: "asc" }, { name: "asc" }],
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
