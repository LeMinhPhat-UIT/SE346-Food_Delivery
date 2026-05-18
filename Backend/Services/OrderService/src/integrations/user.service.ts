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
  storeLogoUrl?: string | null;
};

export type UserAddress = {
  id: string;
  userId: string;
  addressLine: string;
  ward?: string | null;
  district?: string | null;
  city?: string | null;
  lat?: number | null;
  lng?: number | null;
  label?: string | null;
  recipientName?: string | null;
  phone?: string | null;
  isDefault: boolean;
};

export type MerchantAddress = {
  id: string;
  merchantId: string;
  addressLine: string;
  ward?: string | null;
  district?: string | null;
  city?: string | null;
  lat?: number | null;
  lng?: number | null;
};

export class UserServiceClient {
  private async parseResponse<T>(response: Response, message: string) {
    if (!response.ok) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, message);
    }

    return (await response.json()) as ApiResponse<T>;
  }

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

      const payload = await this.parseResponse<PagedResult<MerchantProfile>>(
        response,
        "Failed to resolve merchant context from User Service",
      );
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

  async getUserAddressById(
    userId: string,
    addressId: string,
    token: string,
  ): Promise<UserAddress> {
    const response = await fetch(
      `${env.USER_SERVICE_URL}/${userId}/addresses/${addressId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    const payload = await this.parseResponse<UserAddress>(
      response,
      "Failed to fetch user address from User Service",
    );

    if (!payload.data) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "User address was not found");
    }

    return payload.data;
  }

  async getMerchantPrimaryAddress(merchantId: string): Promise<MerchantAddress> {
    const url = new URL(`${env.USER_SERVICE_URL}/merchant/${merchantId}/location`);
    url.searchParams.set("pageIndex", "1");
    url.searchParams.set("pageSize", "1");

    const response = await fetch(url);
    const payload = await this.parseResponse<PagedResult<MerchantAddress>>(
      response,
      "Failed to fetch merchant address from User Service",
    );

    const address = payload.data?.items?.[0];

    if (!address) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Merchant address was not found");
    }

    return address;
  }

  async getMerchantById(merchantId: string): Promise<MerchantProfile> {
    const response = await fetch(`${env.USER_SERVICE_URL}/merchants/${merchantId}`);
    const payload = await this.parseResponse<MerchantProfile>(
      response,
      "Failed to fetch merchant profile from User Service",
    );

    if (!payload.data) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Merchant profile was not found");
    }

    return payload.data;
  }
}
