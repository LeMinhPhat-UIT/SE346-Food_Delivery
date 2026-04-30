import { z } from "zod";
import {
  categoryQuerySchema,
  createCategoryBodySchema,
  updateCategoryBodySchema,
  updateCategoryStatusBodySchema,
} from "./category.schema";

export type CreateCategoryDto = z.infer<typeof createCategoryBodySchema>;
export type UpdateCategoryDto = z.infer<typeof updateCategoryBodySchema>;
export type CategoryQueryDto = z.infer<typeof categoryQuerySchema>;
export type UpdateCategoryStatusDto = z.infer<
  typeof updateCategoryStatusBodySchema
>;

export type CategoryResponseDto = {
  id: string;
  name: string;
  description: string | null;
  iconUrl: string | null;
  parentId: string | null;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  deletedAt: string | null;
  parent: {
    id: string;
    name: string;
  } | null;
  children: Array<{
    id: string;
    name: string;
  }>;
  productCount: number;
};

export type CategoryTreeNodeDto = CategoryResponseDto & {
  children: CategoryTreeNodeDto[];
};

export type CategoryListResponseDto = {
  items: CategoryResponseDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
};
