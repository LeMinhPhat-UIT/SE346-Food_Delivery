import { ReviewResponseDto } from "./review.dto";
import { ReviewRecord } from "./review.repository";

const normalizeImages = (images: ReviewRecord["images"]) => {
  if (!Array.isArray(images)) {
    return null;
  }

  return images.filter((image): image is string => typeof image === "string");
};

export const toReviewResponseDto = (review: ReviewRecord): ReviewResponseDto => {
  return {
    id: review.id,
    userId: review.userId,
    orderId: review.orderId,
    merchantId: review.merchantId,
    productId: review.productId,
    shipperId: review.shipperId,
    rating: review.rating,
    comment: review.comment,
    images: normalizeImages(review.images),
    merchantReply: review.merchantReply,
    repliedAt: review.repliedAt ? review.repliedAt.toISOString() : null,
    createdAt: review.createdAt.toISOString(),
    deletedAt: review.deletedAt ? review.deletedAt.toISOString() : null,
    product: review.product
      ? {
          id: review.product.id,
          name: review.product.name,
        }
      : null,
  };
};
