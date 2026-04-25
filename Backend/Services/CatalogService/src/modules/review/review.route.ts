import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
import { ReviewController } from "./review.controller";
import {
  createReviewSchema,
  listReviewsSchema,
  reviewIdSchema,
  updateReviewSchema,
} from "./review.schema";

const router = Router();
const reviewController = new ReviewController();

router.get("/", validate(listReviewsSchema), reviewController.getAllReviews);

router.get("/:id", validate(reviewIdSchema), reviewController.getReviewById);

router.post("/", validate(createReviewSchema), reviewController.createReview);

router.put("/:id", validate(updateReviewSchema), reviewController.updateReview);

router.patch(
  "/:id",
  validate(updateReviewSchema),
  reviewController.updateReview,
);

router.delete("/:id", validate(reviewIdSchema), reviewController.deleteReview);

export default router;
