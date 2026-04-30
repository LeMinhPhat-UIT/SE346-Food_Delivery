import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { ReviewController } from "./review.controller";
import {
  createReviewSchema,
  listReviewsSchema,
  restoreReviewSchema,
  reviewMerchantIdSchema,
  reviewIdSchema,
  reviewProductIdSchema,
  reviewReplySchema,
  reviewUserIdSchema,
  updateReviewSchema,
} from "./review.schema";

const router = Router();
const reviewController = new ReviewController();

router.get("/", validate(listReviewsSchema), reviewController.getAllReviews);

router.get(
  "/product/:productId",
  validate(reviewProductIdSchema),
  reviewController.getProductReviews,
);

router.get(
  "/product/:productId/summary",
  validate(reviewProductIdSchema),
  reviewController.getProductReviewSummary,
);

router.get(
  "/user/:userId",
  validate(reviewUserIdSchema),
  reviewController.getUserReviews,
);

router.get(
  "/merchant/:merchantId",
  validate(reviewMerchantIdSchema),
  reviewController.getMerchantReviews,
);

router.get("/:id", validate(reviewIdSchema), reviewController.getReviewById);

router.post("/", validate(createReviewSchema), reviewController.createReview);

router.put("/:id", validate(updateReviewSchema), reviewController.updateReview);

router.patch(
  "/:id",
  validate(updateReviewSchema),
  reviewController.updateReview,
);

router.patch(
  "/:id/reply",
  validate(reviewReplySchema),
  reviewController.replyToReview,
);

router.delete(
  "/:id/reply",
  validate(reviewIdSchema),
  reviewController.deleteReviewReply,
);

router.patch(
  "/:id/restore",
  validate(restoreReviewSchema),
  reviewController.restoreReview,
);

router.delete("/:id", validate(reviewIdSchema), reviewController.deleteReview);

export default router;
