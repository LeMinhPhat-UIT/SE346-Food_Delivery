import { Request, Response } from "express";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import { asyncHandler } from "../../utils/asyncHandler";
import Send from "../../utils/response";
import {
  CancelOrderDto,
  CheckoutPreviewDto,
  CreateOrderDto,
  MyOrdersQueryDto,
  UpdateOrderStatusDto,
} from "./order.dto";
import { orderService } from "./order.bootstrap";

export class OrderController {
  private getAuthContext(req: Request) {
    if (!req.auth?.userId || !req.auth.token) {
      throw new ApiError(HTTP_STATUS.UNAUTHORIZED, "Invalid user context");
    }

    return {
      userId: req.auth.userId,
      token: req.auth.token,
      merchantId: req.auth.merchantId,
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

  createOrder = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const payload = req.validated?.body as CreateOrderDto;
    const order = await orderService.createOrder(
      auth.userId,
      auth.token,
      payload,
    );

    return Send.success(res, order, "Order created successfully", HTTP_STATUS.CREATED);
  });

  getMyOrders = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as MyOrdersQueryDto;
    const orders = await orderService.getMyOrders(auth.userId, query);

    return Send.success(res, orders, "Orders fetched successfully");
  });

  getOrderById = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { id } = req.validated?.params as { id: string };
    const order = await orderService.getOrderById(auth.userId, id);

    return Send.success(res, order, "Order fetched successfully");
  });

  cancelMyOrder = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as CancelOrderDto;
    const order = await orderService.cancelMyOrder(auth.userId, id, payload);

    return Send.success(res, order, "Order cancelled successfully");
  });

  getMerchantOrders = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const query = (req.validated?.query ?? {}) as MyOrdersQueryDto;

    if (!auth.merchantId) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant context is missing");
    }

    const orders = await orderService.getMerchantOrders(auth.merchantId, query);

    return Send.success(res, orders, "Merchant orders fetched successfully");
  });

  getMerchantOrderById = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { id } = req.validated?.params as { id: string };

    if (!auth.merchantId) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant context is missing");
    }

    const order = await orderService.getMerchantOrderById(auth.merchantId, id);

    return Send.success(res, order, "Merchant order fetched successfully");
  });

  updateMerchantOrderStatus = asyncHandler(async (req: Request, res: Response) => {
    const auth = this.getAuthContext(req);
    const { id } = req.validated?.params as { id: string };
    const payload = req.validated?.body as UpdateOrderStatusDto;

    if (!auth.merchantId) {
      throw new ApiError(HTTP_STATUS.FORBIDDEN, "Merchant context is missing");
    }

    const order = await orderService.updateMerchantOrderStatus(
      auth.merchantId,
      auth.userId,
      id,
      payload,
    );

    return Send.success(res, order, "Merchant order status updated successfully");
  });
}
