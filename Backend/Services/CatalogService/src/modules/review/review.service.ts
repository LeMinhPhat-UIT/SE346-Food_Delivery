import { Prisma } from "@prisma/client";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { OrderServiceClient } from "../../integrations/order.service";
import { ProductRepository } from "../product/product.repository";
import { UploadService } from "../upload/upload.service";
import {
  CreateReviewDto,
  ReviewQueryDto,
  ReviewListResponseDto,
  ReviewReplyDto,
  ReviewResponseDto,
  ReviewSummaryDto,
  UpdateReviewDto,
} from "./review.dto";
import { toReviewResponseDto } from "./review.mapper";
import { ReviewRepository } from "./review.repository";

export class ReviewService {
  private readonly uploadService = new UploadService();
  private readonly orderServiceClient = new OrderServiceClient();

  constructor(
    private readonly reviewRepository: ReviewRepository,
    private readonly productRepository: ProductRepository
  ) {}

  async getAllReviews(filters: ReviewQueryDto): Promise<ReviewListResponseDto> {
    if (filters.productId) {
      await this.ensureProductExists(filters.productId);
    }

    const { items, totalCount } = await this.reviewRepository.findAll(filters);
    const limit = filters.limit || 10;
    const page = filters.page || 1;

    return {
      items: items.map(toReviewResponseDto),
      totalCount,
      page,
      limit,
      totalPages: Math.ceil(totalCount / limit),
    };
  }

  async getProductReviews(
    productId: string,
    filters: ReviewQueryDto,
  ): Promise<ReviewListResponseDto> {
    await this.ensureProductExists(productId);

    return this.getAllReviews({
      ...filters,
      productId,
    });
  }

  async getUserReviews(
    userId: string,
    filters: ReviewQueryDto,
  ): Promise<ReviewListResponseDto> {
    return this.getAllReviews({
      ...filters,
      userId,
    });
  }

  async getMerchantReviews(
    merchantId: string,
    filters: ReviewQueryDto,
  ): Promise<ReviewListResponseDto> {
    return this.getAllReviews({
      ...filters,
      merchantId,
    });
  }

  async getReviewById(id: string): Promise<ReviewResponseDto> {
    const review = await this.reviewRepository.findById(id);

    if (!review) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Review not found");
    }

    return toReviewResponseDto(review);
  }

  async createReview(
    userId: string,
    token: string,
    data: CreateReviewDto,
  ): Promise<ReviewResponseDto> {
    const order = await this.orderServiceClient.getOrderById(data.orderId, token);

    if (order.userId !== userId) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "You can only review your own orders",
      );
    }

    if (order.status !== "DELIVERED") {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "You can only review a delivered order",
      );
    }

    const merchantId = order.merchantId;
    if (data.merchantId && data.merchantId !== merchantId) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Merchant ID does not match the reviewed order",
      );
    }

    if (data.productId) {
      await this.ensureProductExists(data.productId);

      const isInOrder = order.items.some((item) => item.productId === data.productId);
      if (!isInOrder) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Product must belong to the reviewed order",
        );
      }
    }

    const normalizedData = this.normalizeCreateReviewPayload({
      ...data,
      userId,
      merchantId,
    });
    const review = await this.reviewRepository.create(normalizedData);

    if (review.productId) {
      await this.reviewRepository.syncProductReviewStats(review.productId);
    }

    return toReviewResponseDto(review);
  }

  async assertMerchantCanReply(reviewId: string, merchantId: string) {
    const review = await this.ensureReviewExists(reviewId);

    if (!review.productId) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Review is not attached to a product",
      );
    }

    const product = await this.productRepository.findById(review.productId);

    if (!product) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Product does not exist");
    }

    if (product.merchantId !== merchantId) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "You can only reply to reviews of your own products",
      );
    }

    return review;
  }

  async assertReviewOwnedByUser(reviewId: string, userId: string) {
    const review = await this.ensureReviewExists(reviewId);

    if (review.userId !== userId) {
      throw new ApiError(
        HTTP_STATUS.FORBIDDEN,
        "You can only modify your own reviews",
      );
    }

    return review;
  }

  async replyToReview(
    id: string,
    data: ReviewReplyDto,
  ): Promise<ReviewResponseDto> {
    await this.ensureReviewExists(id);

    const review = await this.reviewRepository.update(id, {
      merchantReply: data.merchantReply,
      repliedAt: new Date(),
    });

    return toReviewResponseDto(review);
  }

  async deleteReviewReply(id: string): Promise<ReviewResponseDto> {
    await this.ensureReviewExists(id);

    const review = await this.reviewRepository.update(id, {
      merchantReply: null,
      repliedAt: null,
    });

    return toReviewResponseDto(review);
  }

  async restoreReview(id: string): Promise<ReviewResponseDto> {
    const review = await this.reviewRepository.findByIdIncludingDeleted(id);

    if (!review) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Review not found");
    }

    if (review.deletedAt === null) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Review is already active");
    }

    const restoredReview = await this.reviewRepository.update(id, {
      deletedAt: null,
    });

    if (restoredReview.productId) {
      await this.reviewRepository.syncProductReviewStats(restoredReview.productId);
    }

    return toReviewResponseDto(restoredReview);
  }

  async getProductReviewSummary(productId: string): Promise<ReviewSummaryDto> {
    await this.ensureProductExists(productId);

    const { averageRating, totalReviews, grouped } =
      await this.reviewRepository.getProductReviewSummary(productId);

    return {
      productId,
      averageRating,
      totalReviews,
      counts: {
        oneStar: grouped.find((item) => item.rating === 1)?._count._all ?? 0,
        twoStar: grouped.find((item) => item.rating === 2)?._count._all ?? 0,
        threeStar: grouped.find((item) => item.rating === 3)?._count._all ?? 0,
        fourStar: grouped.find((item) => item.rating === 4)?._count._all ?? 0,
        fiveStar: grouped.find((item) => item.rating === 5)?._count._all ?? 0,
      },
    };
  }

  async updateReview(
    id: string,
    data: UpdateReviewDto
  ): Promise<ReviewResponseDto> {
    const existingReview = await this.ensureReviewExists(id);
    const oldImages = this.normalizeImages(existingReview.images);

    const normalizedData = this.normalizeUpdateReviewPayload(data);
    const review = await this.reviewRepository.update(id, normalizedData);

    const affectedProductIds = new Set<string>();

    if (existingReview.productId) {
      affectedProductIds.add(existingReview.productId);
    }

    if (review.productId) {
      affectedProductIds.add(review.productId);
    }

    await Promise.all(
      Array.from(affectedProductIds).map((productId) =>
        this.reviewRepository.syncProductReviewStats(productId)
      )
    );

    if (data.images !== undefined) {
      const nextImages = data.images ?? [];
      const removedImages = oldImages.filter(
        (imageUrl) => !nextImages.includes(imageUrl)
      );

      if (removedImages.length > 0) {
        await this.uploadService.deleteFilesByPublicUrls(removedImages);
      }
    }

    return toReviewResponseDto(review);
  }

  async deleteReview(id: string): Promise<ReviewResponseDto> {
    const existingReview = await this.ensureReviewExists(id);

    const review = await this.reviewRepository.update(id, {
      deletedAt: new Date(),
    });

    if (existingReview.productId) {
      await this.reviewRepository.syncProductReviewStats(existingReview.productId);
    }

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
    data: CreateReviewDto & { userId: string; merchantId: string | null }
  ): Prisma.ReviewUncheckedCreateInput {
    const normalizedData: Prisma.ReviewUncheckedCreateInput = {
      userId: data.userId,
      orderId: data.orderId,
      merchantId: data.merchantId ?? null,
      productId: data.productId ?? null,
      shipperId: data.shipperId ?? null,
      rating: data.rating,
      comment: data.comment ?? null,
      merchantReply: null,
      repliedAt: null,
      images: data.images ?? Prisma.JsonNull,
    };

    return normalizedData;
  }

  private normalizeUpdateReviewPayload(
    data: UpdateReviewDto
  ): Prisma.ReviewUncheckedUpdateInput {
    const normalizedData: Prisma.ReviewUncheckedUpdateInput = {};

    if (data.rating !== undefined) normalizedData.rating = data.rating;
    if (data.comment !== undefined) normalizedData.comment = data.comment;

    if (data.images !== undefined) {
      normalizedData.images = data.images ?? Prisma.JsonNull;
    }

    return normalizedData;
  }

  private normalizeImages(images: Prisma.JsonValue | null) {
    if (!Array.isArray(images)) {
      return [];
    }

    return images.filter((image): image is string => typeof image === "string");
  }
}
