import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { CategoryRepository } from "../category/category.repository";
import { CreateProductDto, UpdateProductDto } from "./product.dto";
import { ProductRepository } from "./product.repository";
import { ProductService } from "./product.service";

const productRepository = new ProductRepository();
const categoryRepository = new CategoryRepository();
const productService = new ProductService(productRepository, categoryRepository);

export class ProductController {
  getAllProducts = asyncHandler(async (_req: Request, res: Response) => {
    const products = await productService.getAllProducts();
    return Send.success(res, products, "Products fetched successfully");
  });

  getProductById = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.getProductById(id);

    return Send.success(res, product, "Product fetched successfully");
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

  deleteProduct = asyncHandler(async (req: Request, res: Response) => {
    const { id } = req.validated?.params as { id: string };
    const product = await productService.deleteProduct(id);

    return Send.success(res, product, "Product deleted successfully");
  });
}
