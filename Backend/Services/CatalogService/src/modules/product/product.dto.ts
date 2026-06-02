import { z } from "zod";
import {
  batchUpdateProductAvailabilityBodySchema,
  createProductBodySchema,
  createProductOptionSchema,
  productOptionIdParamSchema,
  productTaxonomySchema,
  updateProductBodySchema,
  productOptionSchema,
  productOptionValueSchema,
  productQuerySchema,
  updateProductAvailabilityBodySchema,
  updateProductFeaturedBodySchema,
} from "./product.schema";

export type CreateProductDto = z.infer<typeof createProductBodySchema>;
export type UpdateProductDto = z.infer<typeof updateProductBodySchema>;
export type CreateProductOptionDto = z.infer<typeof productOptionSchema>;
export type CreateProductOptionValueDto = z.infer<typeof productOptionValueSchema>;
export type ProductQueryDto = z.infer<typeof productQuerySchema>;
export type ProductTaxonomyDto = z.infer<typeof productTaxonomySchema>;
export type UpdateProductAvailabilityDto = z.infer<
  typeof updateProductAvailabilityBodySchema
>;
export type UpdateProductFeaturedDto = z.infer<
  typeof updateProductFeaturedBodySchema
>;
export type CreateProductOptionRequestDto = z.infer<
  typeof createProductOptionSchema.body
>;
export type UpdateProductOptionDto = z.infer<typeof productOptionSchema>;
export type ProductOptionIdParamDto = z.infer<typeof productOptionIdParamSchema>;
export type BatchUpdateProductAvailabilityDto = z.infer<
  typeof batchUpdateProductAvailabilityBodySchema
>;

export type ProductOptionResponseDto = {
  id: string;
  categoryId: string | null;
  name: string;
  isRequired: boolean;
  maxSelections: number;
  createdAt: string;
  values: Array<{
    id: string;
    name: string;
    additionalPrice: number;
    isAvailable: boolean;
  }>;
};

export type ProductResponseDto = {
  id: string;
  merchantId: string;
  categoryId: string | null;
  taxonomy: ProductTaxonomyDto;
  name: string;
  description: string | null;
  imageUrl: string | null;
  basePrice: number;
  discountPrice: number | null;
  isAvailable: boolean;
  isFeatured: boolean;
  prepTime: number | null;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
  category: {
    id: string;
    name: string;
  } | null;
  reviewCount: number;
  averageRating: number | null;
  options: ProductOptionResponseDto[];
};

export type ProductListResponseDto = {
  items: ProductResponseDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
};
