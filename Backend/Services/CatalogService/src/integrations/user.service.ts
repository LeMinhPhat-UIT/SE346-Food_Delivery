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
    let pageIndex = 1;
    const pageSize = 100;

    while (true) {
      const url = new URL(`${env.USER_SERVICE_URL}/merchants`);
      url.searchParams.set("PageIndex", String(pageIndex));
      url.searchParams.set("PageSize", String(pageSize));

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

      const payload =
        (await response.json()) as UserServiceApiResponse<UserServicePagedResult<MerchantResponse>>;

      if (!payload.success) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          payload.errors?.join(", ") || "Failed to resolve merchant profile"
        );
      }

      const merchant = payload.data.items.find((item) => item.userId === userId);

      if (merchant) {
        return merchant;
      }

      if (pageIndex >= payload.data.totalPages) {
        break;
      }

      pageIndex += 1;
    }

    throw new ApiError(
      HTTP_STATUS.FORBIDDEN,
      "Merchant profile not found for current user"
    );
  }
}
