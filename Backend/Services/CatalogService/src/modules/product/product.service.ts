import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { CategoryRepository } from "../category/category.repository";
import {
  BatchUpdateProductAvailabilityDto,
  CreateProductDto,
  CreateProductOptionRequestDto,
  ProductListResponseDto,
  ProductOptionResponseDto,
  ProductQueryDto,
  ProductResponseDto,
  UpdateProductDto,
  CreateProductOptionDto,
  UpdateProductAvailabilityDto,
  UpdateProductOptionDto,
  UpdateProductFeaturedDto,
} from "./product.dto";
import { toProductOptionResponseDto, toProductResponseDto } from "./product.mapper";
import { ProductRepository } from "./product.repository";

export class ProductService {
  constructor(
    private readonly productRepository: ProductRepository,
    private readonly categoryRepository: CategoryRepository,
  ) {}

  async getAllProducts(filters: ProductQueryDto): Promise<ProductListResponseDto> {
    if (filters.categoryId) {
      await this.ensureCategoryExists(filters.categoryId);
    }

    const { items, totalCount } = await this.productRepository.findAll(filters);

    return {
      items: items.map((product) => toProductResponseDto(product)),
      totalCount,
      page: filters.page,
      limit: filters.limit,
      totalPages: Math.ceil(totalCount / filters.limit),
    };
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

  async createProductOption(
    productId: string,
    data: CreateProductOptionRequestDto,
  ): Promise<ProductOptionResponseDto> {
    await this.ensureProductExists(productId);

    if (data.categoryId) {
      await this.ensureCategoryExists(data.categoryId);
    }

    const option = await this.productRepository.createOption(productId, data);
    return toProductOptionResponseDto(option);
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

  async updateProductOption(
    optionId: string,
    data: UpdateProductOptionDto,
  ): Promise<ProductOptionResponseDto> {
    const existingOption = await this.ensureOptionExists(optionId);

    if (existingOption.productId) {
      await this.ensureProductExists(existingOption.productId);
    }

    if (data.categoryId) {
      await this.ensureCategoryExists(data.categoryId);
    }

    const option = await this.productRepository.updateOption(optionId, data);
    return toProductOptionResponseDto(option);
  }

  async updateProductAvailability(
    id: string,
    data: UpdateProductAvailabilityDto,
  ): Promise<ProductResponseDto> {
    await this.ensureProductExists(id);

    const product = await this.productRepository.update(id, {
      isAvailable: data.isAvailable,
    });

    return toProductResponseDto(product);
  }

  async batchUpdateProductAvailability(
    data: BatchUpdateProductAvailabilityDto,
  ): Promise<ProductResponseDto[]> {
    await Promise.all(
      data.productIds.map((productId) => this.ensureProductExists(productId)),
    );

    const products = await this.productRepository.batchUpdateAvailability(data);

    return products.map((product) => toProductResponseDto(product));
  }

  async updateProductFeatured(
    id: string,
    data: UpdateProductFeaturedDto,
  ): Promise<ProductResponseDto> {
    await this.ensureProductExists(id);

    const product = await this.productRepository.update(id, {
      isFeatured: data.isFeatured,
    });

    return toProductResponseDto(product);
  }

  async restoreProduct(id: string): Promise<ProductResponseDto> {
    const product = await this.productRepository.findByIdIncludingDeleted(id);

    if (!product) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Product not found");
    }

    if (product.deletedAt === null) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Product is already active",
      );
    }

    if (product.categoryId) {
      await this.ensureCategoryExists(product.categoryId);
    }

    const restoredProduct = await this.productRepository.update(id, {
      deletedAt: null,
      isAvailable: true,
    });

    return toProductResponseDto(restoredProduct);
  }

  async deleteProduct(id: string): Promise<ProductResponseDto> {
    await this.ensureProductExists(id);

    const product = await this.productRepository.update(id, {
      isAvailable: false,
      deletedAt: new Date(),
    });

    return toProductResponseDto(product);
  }

  async deleteProductOption(optionId: string): Promise<ProductOptionResponseDto> {
    const existingOption = await this.ensureOptionExists(optionId);

    if (existingOption.productId) {
      await this.ensureProductExists(existingOption.productId);
    }

    const option = await this.productRepository.deleteOption(optionId);
    return toProductOptionResponseDto(option);
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

  private async ensureOptionExists(optionId: string) {
    const option = await this.productRepository.findOptionById(optionId);

    if (!option) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Product option not found");
    }

    return option;
  }
}
