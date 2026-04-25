import { Prisma } from "@prisma/client";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { ProductRepository } from "../product/product.repository";
import {
  CreateReviewDto,
  ReviewQueryDto,
  ReviewResponseDto,
  UpdateReviewDto,
} from "./review.dto";
import { toReviewResponseDto } from "./review.mapper";
import { ReviewRepository } from "./review.repository";

export class ReviewService {
  constructor(
    private readonly reviewRepository: ReviewRepository,
    private readonly productRepository: ProductRepository
  ) {}

  async getAllReviews(filters: ReviewQueryDto): Promise<{
    items: ReviewResponseDto[];
    totalCount: number;
    page: number;
    limit: number;
    totalPages: number;
  }> {
    if (filters.productId) {
      await this.ensureProductExists(filters.productId);
    }

    const { items, totalCount } = await this.reviewRepository.findAll(filters);
    const limit = filters.limit || 10;
    const page = filters.page || 1;

    return {
      items: items.map(toReviewResponseDto),
      totalCount: totalCount,
      page: page,
      limit: limit,
      totalPages: Math.ceil(totalCount / limit)
    };
  }

  async getReviewById(id: string): Promise<ReviewResponseDto> {
    const review = await this.reviewRepository.findById(id);

    if (!review) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Review not found");
    }

    return toReviewResponseDto(review);
  }

  async createReview(data: CreateReviewDto): Promise<ReviewResponseDto> {
    if (data.productId) {
      await this.ensureProductExists(data.productId);
    }

    const normalizedData = this.normalizeCreateReviewPayload(data);
    const review = await this.reviewRepository.create(normalizedData);

    return toReviewResponseDto(review);
  }

  async updateReview(
    id: string,
    data: UpdateReviewDto
  ): Promise<ReviewResponseDto> {
    await this.ensureReviewExists(id);

    if (data.productId !== undefined && data.productId !== null) {
      await this.ensureProductExists(data.productId);
    }

    const normalizedData = this.normalizeUpdateReviewPayload(data);
    const review = await this.reviewRepository.update(id, normalizedData);

    return toReviewResponseDto(review);
  }

  async deleteReview(id: string): Promise<ReviewResponseDto> {
    await this.ensureReviewExists(id);

    const review = await this.reviewRepository.update(id, {
      deletedAt: new Date(),
    });

    return toReviewResponseDto(review);
  }

  private async ensureReviewExists(id: string) {
    const review = await this.reviewRepository.findById(id);

    if (!review) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Review not found");
    }

    return review;
  }

  private async ensureProductExists(productId: string) {
    const product = await this.productRepository.findById(productId);

    if (!product) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Product does not exist");
    }
  }

  private normalizeCreateReviewPayload(
    data: CreateReviewDto
  ): Prisma.ReviewUncheckedCreateInput {
    const normalizedData: Prisma.ReviewUncheckedCreateInput = {
      ...data,
      images: data.images ?? Prisma.JsonNull,
    };

    if (data.merchantReply && data.merchantReply.trim().length > 0) {
      normalizedData.repliedAt = data.repliedAt ?? new Date();
    }

    if (data.merchantReply === null) {
      normalizedData.repliedAt = null;
    }

    return normalizedData;
  }

  private normalizeUpdateReviewPayload(
    data: UpdateReviewDto
  ): Prisma.ReviewUncheckedUpdateInput {
    const normalizedData: Prisma.ReviewUncheckedUpdateInput = {};

    if (data.userId !== undefined) normalizedData.userId = data.userId;
    if (data.orderId !== undefined) normalizedData.orderId = data.orderId;
    if (data.merchantId !== undefined) normalizedData.merchantId = data.merchantId;
    if (data.productId !== undefined) normalizedData.productId = data.productId;
    if (data.shipperId !== undefined) normalizedData.shipperId = data.shipperId;
    if (data.rating !== undefined) normalizedData.rating = data.rating;
    if (data.comment !== undefined) normalizedData.comment = data.comment;

    if (data.images !== undefined) {
      normalizedData.images = data.images ?? Prisma.JsonNull;
    }

    if (data.merchantReply !== undefined) {
      normalizedData.merchantReply = data.merchantReply;

      if (data.merchantReply && data.merchantReply.trim().length > 0) {
        normalizedData.repliedAt = data.repliedAt ?? new Date();
      } else if (data.merchantReply === null) {
        normalizedData.repliedAt = null;
      }
    } else if (data.repliedAt !== undefined) {
      normalizedData.repliedAt = data.repliedAt;
    }

    return normalizedData;
  }
}
