import { ROLES } from "../../constants/roles";
import { authenticate, requireRoles } from "../../middlewares/auth.middleware";
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

router.post(
  "/",
  authenticate,
  requireRoles(ROLES.CUSTOMER, ROLES.ADMIN),
  validate(createReviewSchema),
  reviewController.createReview
);

router.put(
  "/:id",
  authenticate,
  requireRoles(ROLES.CUSTOMER, ROLES.ADMIN),
  validate(updateReviewSchema),
  reviewController.updateReview
);

router.patch(
  "/:id",
  authenticate,
  requireRoles(ROLES.CUSTOMER, ROLES.ADMIN),
  validate(updateReviewSchema),
  reviewController.updateReview,
);

router.patch(
  "/:id/reply",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  validate(reviewReplySchema),
  reviewController.replyToReview,
);

router.delete(
  "/:id/reply",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  validate(reviewIdSchema),
  reviewController.deleteReviewReply,
);

router.patch(
  "/:id/restore",
  authenticate,
  requireRoles(ROLES.ADMIN),
  validate(restoreReviewSchema),
  reviewController.restoreReview,
);

router.delete(
  "/:id",
  authenticate,
  requireRoles(ROLES.CUSTOMER, ROLES.ADMIN),
  validate(reviewIdSchema),
  reviewController.deleteReview
);

export default router;
