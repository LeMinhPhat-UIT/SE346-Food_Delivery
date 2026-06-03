import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

type ApiResponse<T> = {
  success: boolean;
  data?: T;
  errors?: string[];
};

export type EstimateDeliveryFeePayload = {
  pickupLat: number;
  pickupLng: number;
  deliveryLat: number;
  deliveryLng: number;
  subtotal: number;
};

export type EstimateDeliveryFeeResult = {
  distanceKm: number;
  estimatedTimeMinutes: number;
  baseFee: number;
  distanceFee: number;
  deliveryFee: number;
  currency: string;
  isWithinDeliveryRadius: boolean;
  maxDeliveryDistanceKm: number;
};

type EstimateDeliveryFeeResponse = {
  distanceKm: number;
  estimatedTimeMinutes: number;
  baseFee: number;
  distanceFee: number;
  deliveryFee: number;
  currency: string;
  isWithinDeliveryRadius: boolean;
  maxDeliveryDistanceKm: number;
};

export class DeliveryServiceClient {
  async estimateDeliveryFee(
    payload: EstimateDeliveryFeePayload,
    token?: string,
  ): Promise<EstimateDeliveryFeeResult> {
    const response = await fetch(`${env.DELIVERY_SERVICE_URL}/estimate-fee`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : {}),
      },
      body: JSON.stringify({
        pickupLat: payload.pickupLat,
        pickupLng: payload.pickupLng,
        deliveryLat: payload.deliveryLat,
        deliveryLng: payload.deliveryLng,
        subtotal: payload.subtotal,
      }),
    });

    if (!response.ok) {
      const errorPayload = await this.tryParseApiResponse(response);

      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        errorPayload?.errors?.join(", ") ||
          "Failed to estimate delivery fee from Delivery Service",
      );
    }

    const result = (await response.json()) as ApiResponse<EstimateDeliveryFeeResponse>;

    if (!result.success || !result.data) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        result.errors?.join(", ") || "Delivery Service returned invalid estimate response",
      );
    }

    return {
      distanceKm: Number(result.data.distanceKm),
      estimatedTimeMinutes: result.data.estimatedTimeMinutes,
      baseFee: Number(result.data.baseFee),
      distanceFee: Number(result.data.distanceFee),
      deliveryFee: Number(result.data.deliveryFee),
      currency: result.data.currency,
      isWithinDeliveryRadius: result.data.isWithinDeliveryRadius,
      maxDeliveryDistanceKm: Number(result.data.maxDeliveryDistanceKm),
    };
  }

  private async tryParseApiResponse(response: Response): Promise<ApiResponse<unknown> | null> {
    try {
      return (await response.json()) as ApiResponse<unknown>;
    } catch {
      return null;
    }
  }
}
