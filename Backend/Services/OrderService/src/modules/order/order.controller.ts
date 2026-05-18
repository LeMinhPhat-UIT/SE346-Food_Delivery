import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import { UserServiceClient } from "../../integrations/user.service";
import { CatalogServiceClient } from "../../integrations/catalog.service";
import { CartRepository } from "../cart/cart.repository";
import { CartService } from "../cart/cart.service";
import { CheckoutPreviewDto } from "./order.dto";
import { OrderRepository } from "./order.repository";
import { OrderService } from "./order.service";

const orderRepository = new OrderRepository();
const cartRepository = new CartRepository();
const catalogServiceClient = new CatalogServiceClient();
const cartService = new CartService(cartRepository, catalogServiceClient);
const userServiceClient = new UserServiceClient();
const orderService = new OrderService(
  orderRepository,
  cartService,
  userServiceClient,
);

export class OrderController {
  private getAuthContext(req: Request) {
    if (!req.auth?.userId || !req.auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return {
      userId: req.auth.userId,
      token: req.auth.token,
    };
  }

  previewCheckout = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const payload = req.validated?.body as CheckoutPreviewDto;
    const preview = await orderService.previewCheckout(
      auth.userId,
      auth.token,
      payload,
    );

    return Send.success(res, preview, "Checkout preview fetched successfully");
  });
}
