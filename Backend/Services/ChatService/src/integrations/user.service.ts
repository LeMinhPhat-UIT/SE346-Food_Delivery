import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

type ApiResponse<T> = {
  success: boolean;
  data?: T;
  errors?: string[];
};

export type MerchantProfile = {
  id: string;
  userId: string;
  storeName?: string | null;
  storeLogoUrl?: string | null;
};

export type ShipperProfile = {
  id: string;
  userId: string;
};

export class UserServiceClient {
  private async parseResponse<T>(response: Response, message: string) {
    if (!response.ok) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, message);
    }

    return (await response.json()) as ApiResponse<T>;
  }

  async getMerchantByUserId(userId: string, token: string): Promise<MerchantProfile | null> {
    const response = await fetch(`${env.USER_SERVICE_URL}/merchants/by-user/${userId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 404) {
      return null;
    }

    const payload = await this.parseResponse<MerchantProfile>(
      response,
      "Failed to resolve merchant profile from User Service",
    );

    return payload.data ?? null;
  }

  async getShipperByUserId(userId: string, token: string): Promise<ShipperProfile | null> {
    const response = await fetch(`${env.USER_SERVICE_URL}/shippers/by-user/${userId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 404) {
      return null;
    }

    const payload = await this.parseResponse<ShipperProfile>(
      response,
      "Failed to resolve shipper profile from User Service",
    );

    return payload.data ?? null;
  }
}
