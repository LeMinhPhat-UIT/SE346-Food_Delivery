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
    const { page, limit, ...whereFilters } = filters; 
    const skip = (page! - 1) * limit!;

    const [items, totalCount] = await Promise.all([
      prisma.review.findMany({
        where: { deletedAt: null, ...whereFilters },
        orderBy: [{ createdAt: "desc" }],
        skip: skip,
        take: limit,
        select: reviewSelect,
      }),
      prisma.review.count({
        where: { deletedAt: null, ...whereFilters },
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
}
