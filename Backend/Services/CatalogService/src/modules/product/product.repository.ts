import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { CreateProductDto } from "./product.dto";

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
    return prisma.product.create({
      data,
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
}
