import { z } from "zod";
import {
  createProductBodySchema,
  updateProductBodySchema,
  productOptionSchema,
  productOptionValueSchema,
} from "./product.schema";

export type CreateProductDto = z.infer<typeof createProductBodySchema>;
export type UpdateProductDto = z.infer<typeof updateProductBodySchema>;
export type CreateProductOptionDto = z.infer<typeof productOptionSchema>;
export type CreateProductOptionValueDto = z.infer<typeof productOptionValueSchema>;

export type ProductResponseDto = {
  id: string;
  merchantId: string;
  categoryId: string | null;
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
  options: Array<{
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
  }>;
};
