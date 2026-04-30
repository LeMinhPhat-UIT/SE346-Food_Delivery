import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import {
  BatchUpdateProductAvailabilityDto,
  CreateProductDto,
  CreateProductOptionDto,
  ProductQueryDto,
  UpdateProductOptionDto,
} from "./product.dto";

export const productSelect = Prisma.validator<Prisma.ProductSelect>()({
  id: true,
  merchantId: true,
  categoryId: true,
  name: true,
  description: true,
  imageUrl: true,
  basePrice: true,
  discountPrice: true,
  isAvailable: true,
  isFeatured: true,
  prepTime: true,
  averageRating: true,
  totalReviews: true,
  createdAt: true,
  updatedAt: true,
  deletedAt: true,
  category: {
    select: {
      id: true,
      name: true,
    },
  },
  options: {
    select: {
      id: true,
      productId: true,
      categoryId: true,
      name: true,
      isRequired: true,
      maxSelections: true,
      createdAt: true,
      values: {
        select: {
          id: true,
          name: true,
          additionalPrice: true,
          isAvailable: true,
        },
        orderBy: {
          name: "asc",
        },
      },
    },
    orderBy: {
      createdAt: "asc",
    },
  },
});

export type ProductRecord = Prisma.ProductGetPayload<{
  select: typeof productSelect;
}>;

export const productOptionSelect = Prisma.validator<Prisma.ProductOptionSelect>()({
  id: true,
  productId: true,
  categoryId: true,
  name: true,
  isRequired: true,
  maxSelections: true,
  createdAt: true,
  values: {
    select: {
      id: true,
      name: true,
      additionalPrice: true,
      isAvailable: true,
    },
    orderBy: {
      name: "asc",
    },
  },
});

export type ProductOptionRecord = Prisma.ProductOptionGetPayload<{
  select: typeof productOptionSelect;
}>;

export class ProductRepository {
  async findAll(filters: ProductQueryDto) {
    const {
      page,
      limit,
      search,
      merchantId,
      categoryId,
      isAvailable,
      isFeatured,
      includeDeleted,
      minPrice,
      maxPrice,
      sortBy,
      sortOrder,
    } = filters;

    const where: Prisma.ProductWhereInput = {
      deletedAt: includeDeleted ? undefined : null,
      merchantId,
      categoryId,
      isAvailable,
      isFeatured,
      basePrice: {
        gte: minPrice,
        lte: maxPrice,
      },
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
      prisma.product.findMany({
        where,
        orderBy: {
          [sortBy]: sortOrder,
        },
        skip: (page - 1) * limit,
        take: limit,
        select: productSelect,
      }),
      prisma.product.count({ where }),
    ]);

    return { items, totalCount };
  }

  async findById(id: string) {
    const product = await prisma.product.findUnique({
      where: { id },
      select: productSelect,
    });

    if (!product || product.deletedAt !== null) {
      return null;
    }

    return product;
  }

  async findByIdIncludingDeleted(id: string) {
    return prisma.product.findUnique({
      where: { id },
      select: productSelect,
    });
  }

  async create(data: CreateProductDto) {
    const { options = [], ...productData } = data;

    return prisma.product.create({
      data: {
        ...productData,
        options: {
          create: this.mapOptionCreates(options),
        },
      },
      select: productSelect,
    });
  }

  async update(id: string, data: Prisma.ProductUpdateInput) {
    return prisma.product.update({
      where: { id },
      data,
      select: productSelect,
    });
  }

  async replaceProductWithOptions(
    id: string,
    data: Prisma.ProductUpdateInput,
    options: CreateProductOptionDto[],
  ) {
    return prisma.$transaction(async (tx) => {
      await tx.productOptionValue.deleteMany({
        where: {
          option: {
            productId: id,
          },
        },
      });

      await tx.productOption.deleteMany({
        where: {
          productId: id,
        },
      });

      return tx.product.update({
        where: { id },
        data: {
          ...data,
          options: {
            create: this.mapOptionCreates(options),
          },
        },
        select: productSelect,
      });
    });
  }

  async findOptionById(optionId: string) {
    return prisma.productOption.findUnique({
      where: { id: optionId },
      select: productOptionSelect,
    });
  }

  async createOption(productId: string, data: CreateProductOptionDto) {
    return prisma.productOption.create({
      data: {
        productId,
        categoryId: data.categoryId,
        name: data.name,
        isRequired: data.isRequired,
        maxSelections: data.maxSelections,
        values: {
          create: data.values.map((value) => ({
            name: value.name,
            additionalPrice: value.additionalPrice,
            isAvailable: value.isAvailable,
          })),
        },
      },
      select: productOptionSelect,
    });
  }

  async updateOption(optionId: string, data: UpdateProductOptionDto) {
    return prisma.$transaction(async (tx) => {
      await tx.productOptionValue.deleteMany({
        where: {
          optionId,
        },
      });

      return tx.productOption.update({
        where: { id: optionId },
        data: {
          categoryId: data.categoryId,
          name: data.name,
          isRequired: data.isRequired,
          maxSelections: data.maxSelections,
          values: {
            create: data.values.map((value) => ({
              name: value.name,
              additionalPrice: value.additionalPrice,
              isAvailable: value.isAvailable,
            })),
          },
        },
        select: productOptionSelect,
      });
    });
  }

  async deleteOption(optionId: string) {
    return prisma.$transaction(async (tx) => {
      await tx.productOptionValue.deleteMany({
        where: { optionId },
      });

      return tx.productOption.delete({
        where: { id: optionId },
        select: productOptionSelect,
      });
    });
  }

  async batchUpdateAvailability(data: BatchUpdateProductAvailabilityDto) {
    await prisma.product.updateMany({
      where: {
        id: {
          in: data.productIds,
        },
        deletedAt: null,
      },
      data: {
        isAvailable: data.isAvailable,
      },
    });

    return prisma.product.findMany({
      where: {
        id: {
          in: data.productIds,
        },
        deletedAt: null,
      },
      select: productSelect,
    });
  }

  async syncProductReviewStats(productId: string) {
    const aggregate = await prisma.review.aggregate({
      where: {
        productId,
        deletedAt: null,
      },
      _avg: {
        rating: true,
      },
      _count: {
        _all: true,
      },
    });

    return prisma.product.update({
      where: { id: productId },
      data: {
        averageRating: aggregate._avg.rating ?? 0,
        totalReviews: aggregate._count._all,
      },
      select: productSelect,
    });
  }

  private mapOptionCreates(options: CreateProductOptionDto[]) {
    return options.map((option) => ({
      categoryId: option.categoryId,
      name: option.name,
      isRequired: option.isRequired,
      maxSelections: option.maxSelections,
      values: {
        create: option.values.map((value) => ({
          name: value.name,
          additionalPrice: value.additionalPrice,
          isAvailable: value.isAvailable,
        })),
      },
    }));
  }
}
