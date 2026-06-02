import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

type OrderItemResponse = {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
};

type OrderDetailResponse = {
  id: string;
  orderNumber: string;
  userId: string;
  merchantId: string;
  status: string;
  paymentStatus: string;
  items: OrderItemResponse[];
};

type OrderServiceApiResponse<T> = {
  ok?: boolean;
  success?: boolean;
  message?: string;
  data?: T;
};

export class OrderServiceClient {
  async getOrderById(orderId: string, token: string) {
    const url = new URL(`${env.ORDER_SERVICE_URL}/${orderId}`);

    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Failed to resolve order from Order Service",
      );
    }

    const payload = (await response.json()) as OrderServiceApiResponse<OrderDetailResponse>;
    const order = payload.data;

    if (!order) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        payload.message || "Failed to resolve order from Order Service",
      );
    }

    return order;
  }
}
