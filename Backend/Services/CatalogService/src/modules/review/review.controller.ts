import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ROLES } from "../../constants/roles";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { ProductRepository } from "../product/product.repository";
import {
  CreateReviewDto,
  ReviewQueryDto,
  ReviewReplyDto,
  UpdateReviewDto,
} from "./review.dto";
import { ReviewRepository } from "./review.repository";
import { ReviewService } from "./review.service";

const reviewRepository = new ReviewRepository();
const productRepository = new ProductRepository();
const reviewService = new ReviewService(reviewRepository, productRepository);

export class ReviewController {
  private async ensureMerchantCanReply(req: Request, reviewId: string) {
    if (
      req.auth?.roles.includes(ROLES.MERCHANT) &&
      !req.auth.roles.includes(ROLES.ADMIN)
    ) {
      const merchantId = req.auth.merchantId;

      if (!merchantId) {
        throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant context is missing");
      }

      await reviewService.assertMerchantCanReply(reviewId, merchantId);
    }
  }

  private async ensureReviewOwnership(req: Request, reviewId: string) {
    if (!req.auth || req.auth.roles.includes(ROLES.ADMIN)) {
      return;
    }

    await reviewService.assertReviewOwnedByUser(reviewId, req.auth.userId);
  }

  getAllReviews = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as ReviewQueryDto;
    const reviews = await reviewService.getAllReviews(filters);

    return Send.success(res, reviews, "Reviews fetched successfully");
  });

  getProductReviews = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as ReviewQueryDto;
    const { productId } = req.validated?.params as { productId: string };
    const reviews = await reviewService.getProductReviews(productId, filters);

    return Send.success(res, reviews, "Product reviews fetched successfully");
  });

  getUserReviews = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as ReviewQueryDto;
    const { userId } = req.validated?.params as { userId: string };
    const reviews = await reviewService.getUserReviews(userId, filters);

    return Send.success(res, reviews, "User reviews fetched successfully");
  });

  getMerchantReviews = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as ReviewQueryDto;
    const { merchantId } = req.validated?.params as { merchantId: string };
    const reviews = await reviewService.getMerchantReviews(merchantId, filters);

    return Send.success(res, reviews, "Merchant reviews fetched successfully");
  });

  getReviewById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const review = await reviewService.getReviewById(id);

    return Send.success(res, review, "Review fetched successfully");
  });

  createReview = asyncHandler(async (req: Request, res: Response) => {
    const auth = req.auth;

    if (!auth?.userId || !auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    const payload = req.validated?.body as CreateReviewDto;
    const review = await reviewService.createReview(auth.userId, auth.token, payload);

    return Send.success(
      res,
      review,
      "Review created successfully",
      HTTP_STATUS.CREATED
    );
  });

  updateReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    await this.ensureReviewOwnership(req, id);
    const payload = req.validated?.body as UpdateReviewDto;
    const review = await reviewService.updateReview(id, payload);

    return Send.success(res, review, "Review updated successfully");
  });

  replyToReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    await this.ensureMerchantCanReply(req, id);
    const payload = req.validated?.body as ReviewReplyDto;
    const review = await reviewService.replyToReview(id, payload);

    return Send.success(res, review, "Review reply updated successfully");
  });

  deleteReviewReply = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    await this.ensureMerchantCanReply(req, id);
    const review = await reviewService.deleteReviewReply(id);

    return Send.success(res, review, "Review reply deleted successfully");
  });

  restoreReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const review = await reviewService.restoreReview(id);

    return Send.success(res, review, "Review restored successfully");
  });

  getProductReviewSummary = asyncHandler(async (req: Request, res: Response) => {
    const { productId } = req.validated?.params as { productId: string };
    const summary = await reviewService.getProductReviewSummary(productId);

    return Send.success(res, summary, "Product review summary fetched successfully");
  });

  deleteReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    await this.ensureReviewOwnership(req, id);
    const review = await reviewService.deleteReview(id);

    return Send.success(res, review, "Review deleted successfully");
  });
}
