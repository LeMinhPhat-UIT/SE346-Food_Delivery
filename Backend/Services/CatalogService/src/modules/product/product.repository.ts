import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { CreateProductDto, CreateProductOptionDto } from "./product.dto";

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

export class ProductRepository {
  async findAll() {
    return prisma.product.findMany({
      where: {
        deletedAt: null,
      },
      orderBy: [{ createdAt: "desc" }],
      select: productSelect,
    });
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
