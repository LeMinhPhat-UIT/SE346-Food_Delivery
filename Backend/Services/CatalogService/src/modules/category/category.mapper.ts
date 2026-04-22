import { CategoryResponseDto } from "./category.dto";
import { CategoryRecord } from "./category.repository";

export const toCategoryResponseDto = (
  category: CategoryRecord
): CategoryResponseDto => {
  return {
    id: category.id,
    name: category.name,
    description: category.description,
    iconUrl: category.iconUrl,
    parentId: category.parentId,
    sortOrder: category.sortOrder,
    isActive: category.isActive,
    createdAt: category.createdAt.toISOString(),
    deletedAt: category.deletedAt ? category.deletedAt.toISOString() : null,
    parent: category.parent
      ? {
          id: category.parent.id,
          name: category.parent.name,
        }
      : null,
    children: category.children.map((child) => ({
      id: child.id,
      name: child.name,
    })),
  };
};
