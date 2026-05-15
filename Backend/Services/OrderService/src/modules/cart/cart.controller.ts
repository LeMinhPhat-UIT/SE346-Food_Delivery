import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { CatalogServiceClient } from "../../integrations/catalog.service";
import { AddCartItemDto, UpdateCartItemDto } from "./cart.dto";
import { CartRepository } from "./cart.repository";
import { CartService } from "./cart.service";

const cartRepository = new CartRepository();
const catalogServiceClient = new CatalogServiceClient();
const cartService = new CartService(cartRepository, catalogServiceClient);

export class CartController {
  private getCurrentUserId(req: Request) {
    const userId = req.auth?.userId;

    if (!userId) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return userId;
  }

  getMyCarts = asyncHandler(async (req: Request, res: Response) => {
    const carts = await cartService.getMyCarts(this.getCurrentUserId(req));
    return Send.success(res, carts, "Carts fetched successfully");
  });

  getCartByMerchant = asyncHandler(async (req: Request, res: Response) => {
    const { merchantId } = req.validated?.params as { merchantId: string };
    const cart = await cartService.getCartByMerchant(
      this.getCurrentUserId(req),
      merchantId,
    );

    return Send.success(res, cart, "Merchant cart fetched successfully");
  });

  addCartItem = asyncHandler(async (req: Request, res: Response) => {
    const cart = await cartService.addItem(
      this.getCurrentUserId(req),
      req.validated?.body as AddCartItemDto,
    );

    return Send.success(res, cart, "Item added to cart successfully", HTTP_STATUS.CREATED);
  });

  updateCartItem = asyncHandler(async (req: Request, res: Response) => {
    const { itemId } = req.validated?.params as { itemId: string };
    const cart = await cartService.updateItem(
      this.getCurrentUserId(req),
      itemId,
      req.validated?.body as UpdateCartItemDto,
    );

    return Send.success(res, cart, "Cart item updated successfully");
  });

  removeCartItem = asyncHandler(async (req: Request, res: Response) => {
    const { itemId } = req.validated?.params as { itemId: string };
    const cart = await cartService.removeItem(this.getCurrentUserId(req), itemId);

    return Send.success(res, cart, "Cart item removed successfully");
  });

  clearCart = asyncHandler(async (req: Request, res: Response) => {
    await cartService.clearAllCarts(this.getCurrentUserId(req));
    return Send.success(res, null, "All carts cleared successfully");
  });

  clearCartByMerchant = asyncHandler(async (req: Request, res: Response) => {
    const { merchantId } = req.validated?.params as { merchantId: string };
    await cartService.clearCartByMerchant(this.getCurrentUserId(req), merchantId);
    return Send.success(res, null, "Merchant cart cleared successfully");
  });
}
