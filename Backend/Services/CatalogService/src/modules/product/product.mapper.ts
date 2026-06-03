import { ProductOptionResponseDto, ProductResponseDto } from "./product.dto";
import { ProductOptionRecord, ProductRecord } from "./product.repository";

export const toProductOptionResponseDto = (
  option: ProductOptionRecord,
): ProductOptionResponseDto => {
  return {
    id: option.id,
    categoryId: option.categoryId,
    name: option.name,
    isRequired: option.isRequired,
    maxSelections: option.maxSelections,
    createdAt: option.createdAt.toISOString(),
    values: option.values.map((value) => ({
      id: value.id,
      name: value.name,
      additionalPrice: value.additionalPrice.toNumber(),
      isAvailable: value.isAvailable,
    })),
  };
};

export const toProductResponseDto = (
  product: ProductRecord,
): ProductResponseDto => {
  return {
    id: product.id,
    merchantId: product.merchantId,
    categoryId: product.categoryId,
    taxonomy: product.taxonomy,
    name: product.name,
    description: product.description,
    imageUrl: product.imageUrl,
    basePrice: product.basePrice.toNumber(),
    discountPrice:
      product.discountPrice !== null ? product.discountPrice.toNumber() : null,
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
    reviewCount: product.totalReviews,
    averageRating:
      product.averageRating !== null ? Number(product.averageRating) : null,
    options: product.options?.map(toProductOptionResponseDto),
  };
};
