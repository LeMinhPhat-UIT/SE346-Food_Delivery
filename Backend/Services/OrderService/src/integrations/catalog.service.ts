import { HTTP_STATUS } from "../constants/httpStatus";
import { env } from "../config/env.config";
import { ApiError } from "../utils/apiError";

type ApiResponse<T> = {
  ok?: boolean;
  success?: boolean;
  data?: T;
  message?: string;
};

export type CatalogProductOptionValue = {
  id: string;
  name: string;
  additionalPrice: number;
  isAvailable: boolean;
};

export type CatalogProductOption = {
  id: string;
  name: string;
  isRequired: boolean;
  maxSelections: number;
  values: CatalogProductOptionValue[];
};

export type CatalogProductDetail = {
  id: string;
  merchantId: string;
  name: string;
  imageUrl: string | null;
  basePrice: number;
  discountPrice: number | null;
  isAvailable: boolean;
  deletedAt: string | null;
  options: CatalogProductOption[];
};

export class CatalogServiceClient {
  async getProductDetail(productId: string): Promise<CatalogProductDetail> {
    const response = await fetch(`${env.CATALOG_SERVICE_URL}/products/${productId}/detail`);

    if (!response.ok) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Failed to fetch product from Catalog Service");
    }

    const payload = (await response.json()) as ApiResponse<CatalogProductDetail>;

    if (!payload.data) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Product not found in Catalog Service");
    }

    return payload.data;
  }
}
