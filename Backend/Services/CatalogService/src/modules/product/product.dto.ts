import { z } from "zod";
import {
  createProductBodySchema,
  updateProductBodySchema,
} from "./product.schema";

export type CreateProductDto = z.infer<typeof createProductBodySchema>;
export type UpdateProductDto = z.infer<typeof updateProductBodySchema>;

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
};
