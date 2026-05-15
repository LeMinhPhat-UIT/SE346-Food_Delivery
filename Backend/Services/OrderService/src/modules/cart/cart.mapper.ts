import {
  CartListResponseDto,
  CartResponseDto,
  CartStoredRecord,
} from "./cart.dto";

export const toCartResponseDto = (cart: CartStoredRecord): CartResponseDto => {
  const totalQuantity = cart.items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = cart.items.reduce((sum, item) => sum + item.lineTotal, 0);

  return {
    userId: cart.userId,
    merchantId: cart.merchantId,
    items: cart.items.map(({ signature: _signature, ...item }) => item),
    totalQuantity,
    subtotal,
    updatedAt: cart.updatedAt,
  };
};

export const toCartListResponseDto = (
  carts: CartStoredRecord[],
): CartListResponseDto => {
  const items = carts
    .map(toCartResponseDto)
    .sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));

  return {
    items,
    totalCount: items.length,
  };
};
