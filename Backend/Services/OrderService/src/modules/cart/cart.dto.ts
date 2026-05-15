import { z } from "zod";
import {
  addCartItemBodySchema,
  updateCartItemBodySchema,
} from "./cart.schema";

export type AddCartItemDto = z.infer<typeof addCartItemBodySchema>;
export type UpdateCartItemDto = z.infer<typeof updateCartItemBodySchema>;

export type CartSelectedValueDto = {
  valueId: string;
  name: string;
  additionalPrice: number;
};

export type CartSelectedOptionDto = {
  optionId: string;
  name: string;
  values: CartSelectedValueDto[];
};

export type CartItemResponseDto = {
  id: string;
  productId: string;
  merchantId: string;
  productName: string;
  productImage: string | null;
  note: string | null;
  quantity: number;
  baseUnitPrice: number;
  unitPrice: number;
  lineTotal: number;
  selectedOptions: CartSelectedOptionDto[];
  addedAt: string;
};

export type CartResponseDto = {
  userId: string;
  merchantId: string;
  items: CartItemResponseDto[];
  totalQuantity: number;
  subtotal: number;
  updatedAt: string;
};

export type CartListResponseDto = {
  items: CartResponseDto[];
  totalCount: number;
};

export type CartStoredItem = {
  id: string;
  productId: string;
  merchantId: string;
  productName: string;
  productImage: string | null;
  note: string | null;
  quantity: number;
  baseUnitPrice: number;
  unitPrice: number;
  lineTotal: number;
  selectedOptions: CartSelectedOptionDto[];
  signature: string;
  addedAt: string;
};

export type CartStoredRecord = {
  userId: string;
  merchantId: string;
  items: CartStoredItem[];
  updatedAt: string;
};
