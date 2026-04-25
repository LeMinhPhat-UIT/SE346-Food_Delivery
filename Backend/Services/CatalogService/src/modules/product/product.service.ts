import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { CategoryRepository } from "../category/category.repository";
import {
  CreateProductDto,
  ProductResponseDto,
  UpdateProductDto,
  CreateProductOptionDto,
} from "./product.dto";
import { toProductResponseDto } from "./product.mapper";
import { ProductRepository } from "./product.repository";

export class ProductService {
  constructor(
    private readonly productRepository: ProductRepository,
    private readonly categoryRepository: CategoryRepository,
  ) {}

  async getAllProducts(): Promise<ProductResponseDto[]> {
    const products = await this.productRepository.findAll();
    return products.map(toProductResponseDto);
  }

  async getProductById(id: string): Promise<ProductResponseDto> {
    const product = await this.productRepository.findById(id);

    if (!product) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Product not found");
    }

    return toProductResponseDto(product);
  }

  async createProduct(data: CreateProductDto): Promise<ProductResponseDto> {
    if (data.categoryId) {
      await this.ensureCategoryExists(data.categoryId);
    }

    await this.validateOptionCategories(data.options);

    const product = await this.productRepository.create(data);
    return toProductResponseDto(product);
  }

  async updateProduct(
    id: string,
    data: UpdateProductDto,
  ): Promise<ProductResponseDto> {
    const existingProduct = await this.ensureProductExists(id);

    if (data.categoryId !== undefined && data.categoryId !== null) {
      await this.ensureCategoryExists(data.categoryId);
    }

    await this.validateOptionCategories(data.options);

    const nextBasePrice =
      data.basePrice ?? existingProduct.basePrice.toNumber();
    const nextDiscountPrice =
      data.discountPrice !== undefined
        ? data.discountPrice
        : (existingProduct.discountPrice?.toNumber() ?? null);

    if (nextDiscountPrice !== null && nextDiscountPrice > nextBasePrice) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Discount price cannot be greater than base price",
      );
    }

    const { options, ...productData } = data;

    const product =
      options !== undefined
        ? await this.productRepository.replaceProductWithOptions(
            id,
            productData,
            options,
          )
        : await this.productRepository.update(id, productData);
        
    return toProductResponseDto(product);
  }

  async deleteProduct(id: string): Promise<ProductResponseDto> {
    await this.ensureProductExists(id);

    const product = await this.productRepository.update(id, {
      isAvailable: false,
      deletedAt: new Date(),
    });

    return toProductResponseDto(product);
  }

  private async ensureProductExists(id: string) {
    const product = await this.productRepository.findById(id);

    if (!product) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Product not found");
    }

    return product;
  }

  private async ensureCategoryExists(categoryId: string) {
    const category = await this.categoryRepository.findById(categoryId);

    if (!category || !category.isActive) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Category does not exist or is inactive",
      );
    }
  }

  private async validateOptionCategories(options?: CreateProductOptionDto[]) {
    if (!options?.length) {
      return;
    }

    const categoryIds = options
      .map((option) => option.categoryId)
      .filter((categoryId): categoryId is string => Boolean(categoryId));

    await Promise.all(
      categoryIds.map((categoryId) => this.ensureCategoryExists(categoryId)),
    );
  }
}
