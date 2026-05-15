import { HTTP_STATUS } from "../constants/httpStatus";
import { env } from "../config/env.config";
import { ApiError } from "../utils/apiError";

type ApiResponse<T> = {
  success: boolean;
  data?: T;
  errors?: string[];
};

type PagedResult<T> = {
  items: T[];
  totalCount: number;
  totalPages: number;
  hasNextPage?: boolean;
  paginationRequest?: {
    pageIndex: number;
    pageSize: number;
  };
};

export type MerchantProfile = {
  id: string;
  userId: string;
  storeName: string;
};

export class UserServiceClient {
  async getMerchantByUserId(
    userId: string,
    token?: string,
  ): Promise<MerchantProfile | null> {
    let pageIndex = 1;
    const pageSize = 100;

    while (true) {
      const url = new URL(`${env.USER_SERVICE_URL}/merchants`);
      url.searchParams.set("pageIndex", String(pageIndex));
      url.searchParams.set("pageSize", String(pageSize));

      const response = await fetch(url, {
        headers: token
          ? {
              Authorization: `Bearer ${token}`,
            }
          : undefined,
      });

      if (!response.ok) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          "Failed to resolve merchant context from User Service",
        );
      }

      const payload = (await response.json()) as ApiResponse<PagedResult<MerchantProfile>>;
      const items = payload.data?.items ?? [];
      const matchedMerchant = items.find((merchant) => merchant.userId === userId);

      if (matchedMerchant) {
        return matchedMerchant;
      }

      const hasNextPage =
        payload.data?.hasNextPage ??
        ((payload.data?.paginationRequest?.pageIndex ?? pageIndex) <
          (payload.data?.totalPages ?? pageIndex));

      if (!hasNextPage) {
        return null;
      }

      pageIndex += 1;
    }
  }
}
