import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { ProductRepository } from "../product/product.repository";
import {
  CreateReviewDto,
  ReviewQueryDto,
  UpdateReviewDto,
} from "./review.dto";
import { ReviewRepository } from "./review.repository";
import { ReviewService } from "./review.service";

const reviewRepository = new ReviewRepository();
const productRepository = new ProductRepository();
const reviewService = new ReviewService(reviewRepository, productRepository);

export class ReviewController {
  getAllReviews = asyncHandler(async (req: Request, res: Response) => {
    const filters = (req.validated?.query ?? {}) as ReviewQueryDto;
    const reviews = await reviewService.getAllReviews(filters);

    return Send.success(res, reviews, "Reviews fetched successfully");
  });

  getReviewById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const review = await reviewService.getReviewById(id);

    return Send.success(res, review, "Review fetched successfully");
  });

  createReview = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as CreateReviewDto;
    const review = await reviewService.createReview(payload);

    return Send.success(
      res,
      review,
      "Review created successfully",
      HTTP_STATUS.CREATED
    );
  });

  updateReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateReviewDto;
    const review = await reviewService.updateReview(id, payload);

    return Send.success(res, review, "Review updated successfully");
  });

  deleteReview = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const review = await reviewService.deleteReview(id);

    return Send.success(res, review, "Review deleted successfully");
  });
}
