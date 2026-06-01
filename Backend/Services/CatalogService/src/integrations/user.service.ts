import { env } from "../config/env.config";
import { HTTP_STATUS } from "../constants/httpStatus";
import { ApiError } from "../utils/apiError";

type MerchantResponse = {
  id: string;
  userId: string;
};

type UserServicePagedResult<T> = {
  items: T[];
  totalPages: number;
};

type UserServiceApiResponse<T> = {
  success: boolean;
  data: T;
  errors?: string[];
};

export class UserServiceClient {
  async getMerchantByUserId(userId: string, token: string) {
    const url = new URL(`${env.USER_SERVICE_URL}/merchants/by-user/${userId}`);

    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        "Failed to resolve merchant profile from User Service"
      );
    }

    const payload = (await response.json()) as UserServiceApiResponse<MerchantResponse>;

    if (!payload.success) {
      throw new ApiError(
        HTTP_STATUS.BAD_REQUEST,
        payload.errors?.join(", ") || "Failed to resolve merchant profile"
      );
    }

    return payload.data;
  }
}
