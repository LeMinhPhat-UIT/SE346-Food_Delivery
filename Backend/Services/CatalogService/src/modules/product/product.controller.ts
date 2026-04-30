import { Request, Response } from "express";
import { validate as isUuid } from "uuid";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { CategoryRepository } from "../category/category.repository";
import {
  BatchUpdateProductAvailabilityDto,
  CreateProductDto,
  CreateProductOptionRequestDto,
  ProductQueryDto,
  UpdateProductOptionDto,
  UpdateProductAvailabilityDto,
  UpdateProductDto,
  UpdateProductFeaturedDto,
} from "./product.dto";
import { ProductRepository } from "./product.repository";
import { ProductService } from "./product.service";

const productRepository = new ProductRepository();
const categoryRepository = new CategoryRepository();
const productService = new ProductService(productRepository, categoryRepository);

export class ProductController {
  private getMerchantIdFromRequest(req: Request) {
    const merchantIdHeader = req.headers["x-merchant-id"];
    const merchantId = Array.isArray(merchantIdHeader)
      ? merchantIdHeader[0]
      : merchantIdHeader;

    if (!merchantId || !isUuid(merchantId)) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Valid x-merchant-id header is required",
      );
    }

    return merchantId;
  }

  getAllProducts = asyncHandler(async (req: Request, res: Response) => {
    const queryFilters = (req.validated?.query ?? {}) as ProductQueryDto;
    const params = (req.validated?.params ?? {}) as { merchantId?: string };
    const filters: ProductQueryDto = {
      ...queryFilters,
      merchantId: params.merchantId ?? queryFilters.merchantId,
    };
    const products = await productService.getAllProducts(filters);
    return Send.success(res, products, "Products fetched successfully");
  });

  getMyProducts = asyncHandler(async (req: Request, res: Response) => {
    const queryFilters = (req.validated?.query ?? {}) as ProductQueryDto;
    const merchantId = this.getMerchantIdFromRequest(req);
    const products = await productService.getAllProducts({
      ...queryFilters,
      merchantId,
    });

    return Send.success(res, products, "Merchant products fetched successfully");
  });

  getProductById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.getProductById(id);

    return Send.success(res, product, "Product fetched successfully");
  });

  getProductDetail = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.getProductById(id);

    return Send.success(res, product, "Product detail fetched successfully");
  });

  createProduct = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as CreateProductDto;
    const product = await productService.createProduct(payload);

    return Send.success(
      res,
      product,
      "Product created successfully",
      HTTP_STATUS.CREATED
    );
  });

  updateProduct = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateProductDto;
    const product = await productService.updateProduct(id, payload);

    return Send.success(res, product, "Product updated successfully");
  });

  createProductOption = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as CreateProductOptionRequestDto;
    const option = await productService.createProductOption(id, payload);

    return Send.success(
      res,
      option,
      "Product option created successfully",
      HTTP_STATUS.CREATED,
    );
  });

  updateProductOption = asyncHandler(async (req: Request, res: Response) => {
    const { optionId } = req.validated?.params as { optionId: string };
    const payload = req.validated?.body as UpdateProductOptionDto;
    const option = await productService.updateProductOption(optionId, payload);

    return Send.success(res, option, "Product option updated successfully");
  });

  updateProductAvailability = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateProductAvailabilityDto;
    const product = await productService.updateProductAvailability(id, payload);

    return Send.success(res, product, "Product availability updated successfully");
  });

  batchUpdateProductAvailability = asyncHandler(async (req: Request, res: Response) => {
    const payload = req.validated?.body as BatchUpdateProductAvailabilityDto;
    const products = await productService.batchUpdateProductAvailability(payload);

    return Send.success(
      res,
      products,
      "Products availability updated successfully",
    );
  });

  updateProductFeatured = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateProductFeaturedDto;
    const product = await productService.updateProductFeatured(id, payload);

    return Send.success(res, product, "Product featured status updated successfully");
  });

  restoreProduct = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.restoreProduct(id);

    return Send.success(res, product, "Product restored successfully");
  });

  deleteProduct = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.deleteProduct(id);

    return Send.success(res, product, "Product deleted successfully");
  });

  deleteProductOption = asyncHandler(async (req: Request, res: Response) => {
    const { optionId } = req.validated?.params as { optionId: string };
    const option = await productService.deleteProductOption(optionId);

    return Send.success(res, option, "Product option deleted successfully");
  });
}
