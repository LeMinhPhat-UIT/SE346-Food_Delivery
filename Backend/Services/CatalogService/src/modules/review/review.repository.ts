import { Prisma } from "@prisma/client";
import { prisma } from "../../prisma/prisma.client";
import { ReviewQueryDto } from "./review.dto";

export const reviewSelect = Prisma.validator<Prisma.ReviewSelect>()({
  id: true,
  userId: true,
  orderId: true,
  merchantId: true,
  productId: true,
  shipperId: true,
  rating: true,
  comment: true,
  images: true,
  merchantReply: true,
  repliedAt: true,
  createdAt: true,
  deletedAt: true,
  product: {
    select: {
      id: true,
      name: true,
    },
  },
});

export type ReviewRecord = Prisma.ReviewGetPayload<{
  select: typeof reviewSelect;
}>;

export class ReviewRepository {
  async findAll(filters: ReviewQueryDto) {
    const {
      page,
      limit,
      sortBy,
      sortOrder,
      hasImages,
      ...whereFilters
    } = filters; 
    const skip = (page! - 1) * limit!;
    const where: Prisma.ReviewWhereInput = {
      deletedAt: null,
      ...whereFilters,
      ...(hasImages === undefined
        ? {}
        : hasImages
          ? {
              NOT: {
                images: {
                  equals: Prisma.JsonNull,
                },
              },
            }
          : {
              OR: [
                {
                  images: {
                    equals: Prisma.JsonNull,
                  },
                },
                {
                  images: {
                    equals: [],
                  },
                },
              ],
            }),
    };

    const [items, totalCount] = await Promise.all([
      prisma.review.findMany({
        where,
        orderBy: [{ [sortBy!]: sortOrder! }],
        skip: skip,
        take: limit,
        select: reviewSelect,
      }),
      prisma.review.count({
        where,
      }),
    ]);

    return { items, totalCount };
  }

  async findById(id: string) {
    const review = await prisma.review.findUnique({
      where: { id },
      select: reviewSelect,
    });

    if (!review || review.deletedAt !== null) {
      return null;
    }

    return review;
  }

  async create(data: Prisma.ReviewUncheckedCreateInput) {
    return prisma.review.create({
      data: {
        ...data,
        images: data.images ?? Prisma.JsonNull,
      },
      select: reviewSelect,
    });
  }

  async update(id: string, data: Prisma.ReviewUncheckedUpdateInput) {
    return prisma.review.update({
      where: { id },
      data,
      select: reviewSelect,
    });
  }

  async findByIdIncludingDeleted(id: string) {
    return prisma.review.findUnique({
      where: { id },
      select: reviewSelect,
    });
  }

  async getProductReviewSummary(productId: string) {
    const [aggregate, grouped] = await Promise.all([
      prisma.review.aggregate({
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
      }),
      prisma.review.groupBy({
        by: ["rating"],
        where: {
          productId,
          deletedAt: null,
        },
        _count: {
          _all: true,
        },
      }),
    ]);

    return {
      averageRating: aggregate._avg.rating ?? 0,
      totalReviews: aggregate._count._all,
      grouped,
    };
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
    });
  }
}
