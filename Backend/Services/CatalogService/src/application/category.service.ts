import { prisma } from "../infrastructure/prisma.client";

export class CategoryService {
    async getAllCategories() {
        return await prisma.category.findMany({
            where: { isActive: true },
            orderBy: { sortOrder: 'asc' }
        });
    }
}