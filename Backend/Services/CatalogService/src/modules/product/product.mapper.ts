import { ProductResponseDto } from "./product.dto";
import { ProductRecord } from "./product.repository";

export const toProductResponseDto = (
  product: ProductRecord
): ProductResponseDto => {
  return {
    id: product.id,
    merchantId: product.merchantId,
    categoryId: product.categoryId,
    name: product.name,
    description: product.description,
    imageUrl: product.imageUrl,
    basePrice: product.basePrice.toNumber(),
    discountPrice: product.discountPrice !== null
      ? product.discountPrice.toNumber()
      : null,
    isAvailable: product.isAvailable,
    isFeatured: product.isFeatured,
    prepTime: product.prepTime,
    createdAt: product.createdAt.toISOString(),
    updatedAt: product.updatedAt.toISOString(),
    deletedAt: product.deletedAt ? product.deletedAt.toISOString() : null,
    category: product.category
      ? {
          id: product.category.id,
          name: product.category.name,
        }
      : null,
  };
};
