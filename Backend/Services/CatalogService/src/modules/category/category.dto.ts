import { z } from "zod";
import { createCategoryBodySchema, updateCategoryBodySchema } from "./category.schema";

export type CreateCategoryDto = z.infer<typeof createCategoryBodySchema>;
export type UpdateCategoryDto = z.infer<typeof updateCategoryBodySchema>;

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
};
