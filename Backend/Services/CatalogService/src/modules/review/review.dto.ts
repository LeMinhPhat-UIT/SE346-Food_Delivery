import { z } from "zod";
import {
  createReviewBodySchema,
  reviewReplyBodySchema,
  reviewQuerySchema,
  updateReviewBodySchema,
} from "./review.schema";

export type CreateReviewDto = z.infer<typeof createReviewBodySchema>;
export type UpdateReviewDto = z.infer<typeof updateReviewBodySchema>;
export type ReviewQueryDto = z.infer<typeof reviewQuerySchema>;
export type ReviewReplyDto = z.infer<typeof reviewReplyBodySchema>;

export type ReviewResponseDto = {
  id: string;
  userId: string;
  orderId: string;
  merchantId: string | null;
  productId: string | null;
  shipperId: string | null;
  rating: number;
  comment: string | null;
  images: string[] | null;
  merchantReply: string | null;
  repliedAt: string | null;
  createdAt: string;
  deletedAt: string | null;
  product: {
    id: string;
    name: string;
  } | null;
};

export type ReviewListResponseDto = {
  items: ReviewResponseDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
};

export type ReviewSummaryDto = {
  productId: string;
  averageRating: number;
  totalReviews: number;
  counts: {
    oneStar: number;
    twoStar: number;
    threeStar: number;
    fourStar: number;
    fiveStar: number;
  };
};
