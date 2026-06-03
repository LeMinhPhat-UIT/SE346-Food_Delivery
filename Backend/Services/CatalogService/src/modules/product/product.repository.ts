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
  taxonomy: true,
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
      taxonomy,
      isAvailable,
      isFeatured,
      includeDeleted,
      minPrice,
      maxPrice,
      sortBy,
      sortOrder,
    } = filters;

    const baseWhere: Prisma.ProductWhereInput = {
      deletedAt: includeDeleted ? undefined : null,
      merchantId,
      categoryId,
      taxonomy,
      isAvailable,
      isFeatured,
      basePrice: {
        gte: minPrice,
        lte: maxPrice,
      },
    };

    if (!search?.trim()) {
      const [items, totalCount] = await Promise.all([
        prisma.product.findMany({
          where: baseWhere,
          orderBy: {
            [sortBy]: sortOrder,
          },
          skip: (page - 1) * limit,
          take: limit,
          select: productSelect,
        }),
        prisma.product.count({ where: baseWhere }),
      ]);

      return { items, totalCount };
    }

    const candidates = await prisma.product.findMany({
      where: baseWhere,
      select: productSelect,
    });

    const ranked = this.rankSearchResults(candidates, search, sortBy, sortOrder);
    const totalCount = ranked.length;
    const items = ranked.slice((page - 1) * limit, page * limit);

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
        taxonomy: productData.taxonomy ?? "OTHER",
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

  private rankSearchResults(
    products: ProductRecord[],
    search: string,
    sortBy: ProductQueryDto["sortBy"],
    sortOrder: ProductQueryDto["sortOrder"],
  ) {
    const normalizedQuery = this.normalizeSearchText(search);
    const tokens = this.tokenizeSearchText(normalizedQuery);
    const queryTaxonomy = this.detectQueryTaxonomy(normalizedQuery, tokens);

    const scored = products
      .map((product) => ({
        product,
        score: this.scoreProductForSearch(
          product,
          normalizedQuery,
          tokens,
          queryTaxonomy,
        ),
      }))
      .filter(({ score }) => score > 0)
      .sort((left, right) => {
        if (right.score !== left.score) {
          return right.score - left.score;
        }

        if (left.product.isFeatured !== right.product.isFeatured) {
          return left.product.isFeatured ? -1 : 1;
        }

        const ratingDiff =
          Number(right.product.averageRating ?? 0) -
          Number(left.product.averageRating ?? 0);
        if (ratingDiff !== 0) {
          return ratingDiff;
        }

        const reviewsDiff = right.product.totalReviews - left.product.totalReviews;
        if (reviewsDiff !== 0) {
          return reviewsDiff;
        }

        return this.compareBySortField(
          left.product,
          right.product,
          sortBy,
          sortOrder,
        );
      });

    return scored.map(({ product }) => product);
  }

  private scoreProductForSearch(
    product: ProductRecord,
    normalizedQuery: string,
    tokens: string[],
    queryTaxonomy: "FOOD" | "DRINK" | "DESSERT" | null,
  ) {
    const normalizedName = this.normalizeSearchText(product.name);
    const normalizedDescription = this.normalizeSearchText(product.description ?? "");
    const normalizedCategory = this.normalizeSearchText(product.category?.name ?? "");
    const normalizedOptionNames = this.normalizeSearchText(
      product.options.map((option) => option.name).join(" "),
    );
    const normalizedOptionValues = this.normalizeSearchText(
      product.options.flatMap((option) => option.values.map((value) => value.name)).join(" "),
    );
    const combinedText = [
      normalizedName,
      normalizedDescription,
      normalizedCategory,
      normalizedOptionNames,
      normalizedOptionValues,
    ]
      .filter(Boolean)
      .join(" ");

    const tokenHits = tokens.filter((token) => this.containsExactWord(combinedText, token)).length;
    
    const nameTokenHits = tokens.filter((token) => this.containsExactWord(normalizedName, token)).length;
    
    const categoryTokenHits = tokens.filter((token) => this.containsExactWord(normalizedCategory, token)).length;
    
    const optionTokenHits = tokens.filter((token) =>
      this.containsExactWord(normalizedOptionNames, token) || 
      this.containsExactWord(normalizedOptionValues, token),
    ).length;

    const textScore =
      (normalizedName.includes(normalizedQuery) ? 120 : 0) +
      (normalizedCategory.includes(normalizedQuery) ? 90 : 0) +
      (normalizedDescription.includes(normalizedQuery) ? 60 : 0) +
      (normalizedOptionNames.includes(normalizedQuery) ||
      normalizedOptionValues.includes(normalizedQuery)
        ? 45
        : 0) +
      (normalizedName.startsWith(normalizedQuery) ? 35 : 0) +
      (combinedText.startsWith(normalizedQuery) ? 15 : 0) +
      nameTokenHits * 18 +
      categoryTokenHits * 12 +
      optionTokenHits * 10 +
      tokenHits * 8;

    const taxonomyScore = queryTaxonomy && product.taxonomy === queryTaxonomy ? 80 : 0;

    if (textScore <= 0 && taxonomyScore === 0) {
      return 0;
    }

    let score = textScore + taxonomyScore;

    if (textScore >= 35) {
      if (product.isFeatured) score += 5;
      score += Number(product.averageRating ?? 0) * 2;
      score += Math.min(product.totalReviews, 100) * 0.05;
    }

    return score;
  }

  private compareBySortField(
    left: ProductRecord,
    right: ProductRecord,
    sortBy: ProductQueryDto["sortBy"],
    sortOrder: ProductQueryDto["sortOrder"],
  ) {
    const direction = sortOrder === "asc" ? 1 : -1;

    const leftValue = this.getSortValue(left, sortBy);
    const rightValue = this.getSortValue(right, sortBy);

    if (leftValue < rightValue) {
      return -1 * direction;
    }

    if (leftValue > rightValue) {
      return 1 * direction;
    }

    return 0;
  }

  private getSortValue(product: ProductRecord, sortBy: ProductQueryDto["sortBy"]) {
    switch (sortBy) {
      case "name":
        return product.name.toLowerCase();
      case "basePrice":
        return Number(product.basePrice);
      case "updatedAt":
        return product.updatedAt.getTime();
      case "averageRating":
        return Number(product.averageRating ?? 0);
      case "totalReviews":
        return product.totalReviews;
      case "createdAt":
      default:
        return product.createdAt.getTime();
    }
  }

  private normalizeSearchText(value: string) {
    return value
      .normalize("NFD")
      .replace(/\p{Diacritic}/gu, "")
      .replace(/đ/g, "d")
      .replace(/Đ/g, "d")
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, " ")
      .replace(/\s+/g, " ")
      .trim();
  }

  private tokenizeSearchText(value: string) {
    if (!value) {
      return [];
    }

    const stopWords = ["do", "mon", "cac", "loai", "su", "cua", "va", "cho"]; 

    return value
      .split(" ")
      .filter((token) => token.length >= 2 && !stopWords.includes(token));
  }

  private containsExactWord(text: string, word: string) {
    const regex = new RegExp(`\\b${word}\\b`, 'i');
    return regex.test(text);
  }

  private detectQueryTaxonomy(
    normalizedQuery: string,
    tokens: string[],
  ): "FOOD" | "DRINK" | "DESSERT" | null {
    const searchable = `${normalizedQuery} ${tokens.join(" ")}`.trim();

    if (!searchable) {
      return null;
    }

    const aliases: Array<["FOOD" | "DRINK" | "DESSERT", string[]]> = [
      ["DRINK", ["do uong", "nuoc", "drink", "beverage", "coffee", "cafe", "tea", "tra", "juice", "soda", "milk tea"]],
      ["DESSERT", ["dessert", "sweet", "cake", "kem", "banh ngot", "trang mieng"]],
      ["FOOD", ["do an", "mon an", "an vat", "food", "meal", "dish", "rice", "com", "pho", "bun", "mien", "mi"]],
    ];

    for (const [taxonomy, hints] of aliases) {
      if (hints.some((hint) => this.containsExactWord(searchable, hint))) {
        return taxonomy;
      }
    }

    return null;
  }
}
