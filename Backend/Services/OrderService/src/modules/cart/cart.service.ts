import { v4 as uuidv4 } from "uuid";
import { CatalogServiceClient } from "../../integrations/catalog.service";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import {
  AddCartItemDto,
  CartListResponseDto,
  CartResponseDto,
  CartSelectedOptionDto,
  CartStoredRecord,
  UpdateCartItemDto,
} from "./cart.dto";
import { toCartListResponseDto, toCartResponseDto } from "./cart.mapper";
import { CartRepository } from "./cart.repository";

export class CartService {
  constructor(
    private readonly cartRepository: CartRepository,
    private readonly catalogServiceClient: CatalogServiceClient,
  ) {}

  async getMyCarts(userId: string): Promise<CartListResponseDto> {
    const carts = await this.cartRepository.findAllByUserId(userId);
    return toCartListResponseDto(carts);
  }

  async getCartByMerchant(
    userId: string,
    merchantId: string,
  ): Promise<CartResponseDto> {
    const cart =
      (await this.cartRepository.findByUserAndMerchantId(userId, merchantId)) ??
      this.createEmptyCart(userId, merchantId);

    return toCartResponseDto(cart);
  }

  async addItem(userId: string, payload: AddCartItemDto): Promise<CartResponseDto> {
    const product = await this.catalogServiceClient.getProductDetail(payload.productId);
    const cart =
      (await this.cartRepository.findByUserAndMerchantId(userId, product.merchantId)) ??
      this.createEmptyCart(userId, product.merchantId);
    const pricing = this.buildSelection(product, payload.selectedOptions, payload.note ?? null);

    if (!product.isAvailable || product.deletedAt !== null) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Product is unavailable");
    }

    const signature = this.buildSignature(product.id, pricing.selectedOptions, payload.note ?? null);
    const existingItem = cart.items.find((item) => item.signature === signature);

    if (existingItem) {
      existingItem.quantity += payload.quantity;
      existingItem.lineTotal = existingItem.unitPrice * existingItem.quantity;
    } else {
      cart.items.push({
        id: uuidv4(),
        productId: product.id,
        merchantId: product.merchantId,
        productName: product.name,
        productImage: product.imageUrl,
        note: payload.note ?? null,
        quantity: payload.quantity,
        baseUnitPrice: product.discountPrice ?? product.basePrice,
        unitPrice: pricing.unitPrice,
        lineTotal: pricing.unitPrice * payload.quantity,
        selectedOptions: pricing.selectedOptions,
        signature,
        addedAt: new Date().toISOString(),
      });
    }

    cart.updatedAt = new Date().toISOString();

    await this.cartRepository.save(cart);
    return toCartResponseDto(cart);
  }

  async updateItem(
    userId: string,
    itemId: string,
    payload: UpdateCartItemDto,
  ): Promise<CartResponseDto> {
    const cart = await this.getExistingCartByItemId(userId, itemId);
    const item = cart.items.find((cartItem) => cartItem.id === itemId);

    if (!item) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Cart item not found");
    }

    const nextNote = payload.note !== undefined ? payload.note : item.note;

    if (payload.selectedOptions !== undefined) {
      const product = await this.catalogServiceClient.getProductDetail(item.productId);
      const pricing = this.buildSelection(product, payload.selectedOptions, nextNote ?? null);

      item.baseUnitPrice = product.discountPrice ?? product.basePrice;
      item.unitPrice = pricing.unitPrice;
      item.selectedOptions = pricing.selectedOptions;
      item.signature = this.buildSignature(item.productId, pricing.selectedOptions, nextNote ?? null);
      item.productImage = product.imageUrl;
      item.productName = product.name;
    }

    if (payload.quantity !== undefined) {
      item.quantity = payload.quantity;
    }

    if (payload.note !== undefined) {
      item.note = payload.note;
    }

    item.lineTotal = item.unitPrice * item.quantity;
    cart.updatedAt = new Date().toISOString();

    await this.cartRepository.save(cart);
    return toCartResponseDto(cart);
  }

  async removeItem(userId: string, itemId: string): Promise<CartResponseDto> {
    const cart = await this.getExistingCartByItemId(userId, itemId);
    const nextItems = cart.items.filter((item) => item.id !== itemId);

    if (nextItems.length === cart.items.length) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Cart item not found");
    }

    const nextCart: CartStoredRecord = {
      ...cart,
      items: nextItems,
      updatedAt: new Date().toISOString(),
    };

    if (nextItems.length === 0) {
      await this.cartRepository.clearByUserAndMerchantId(userId, cart.merchantId);
      return toCartResponseDto(this.createEmptyCart(userId, cart.merchantId));
    }

    await this.cartRepository.save(nextCart);
    return toCartResponseDto(nextCart);
  }

  async clearCartByMerchant(userId: string, merchantId: string): Promise<void> {
    await this.cartRepository.clearByUserAndMerchantId(userId, merchantId);
  }

  async clearAllCarts(userId: string): Promise<void> {
    await this.cartRepository.clearAllByUserId(userId);
  }

  private async getExistingCartByItemId(userId: string, itemId: string) {
    const carts = await this.cartRepository.findAllByUserId(userId);
    const cart = carts.find((candidate) =>
      candidate.items.some((item) => item.id === itemId),
    );

    if (!cart) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Cart item not found");
    }

    return cart;
  }

  private createEmptyCart(userId: string, merchantId: string): CartStoredRecord {
    return {
      userId,
      merchantId,
      items: [],
      updatedAt: new Date().toISOString(),
    };
  }

  private buildSelection(
    product: Awaited<ReturnType<CatalogServiceClient["getProductDetail"]>>,
    selectedOptionsInput: AddCartItemDto["selectedOptions"],
    note: string | null,
  ) {
    const selectedOptionsMap = new Map(
      selectedOptionsInput.map((option) => [option.optionId, option.valueIds]),
    );
    const duplicatedOptionIds = new Set<string>();

    selectedOptionsInput.forEach((option) => {
      if (
        selectedOptionsInput.filter((candidate) => candidate.optionId === option.optionId)
          .length > 1
      ) {
        duplicatedOptionIds.add(option.optionId);
      }
    });

    if (duplicatedOptionIds.size > 0) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Duplicated option selections are not allowed");
    }

    const selectedOptions: CartSelectedOptionDto[] = [];
    let extraPrice = 0;

    for (const option of product.options) {
      const selectedValueIds = selectedOptionsMap.get(option.id) ?? [];

      if (option.isRequired && selectedValueIds.length === 0) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          `Option "${option.name}" is required`,
        );
      }

      if (selectedValueIds.length > option.maxSelections) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          `Option "${option.name}" allows up to ${option.maxSelections} selections`,
        );
      }

      if (selectedValueIds.length === 0) {
        continue;
      }

      const uniqueValueIds = new Set(selectedValueIds);

      if (uniqueValueIds.size !== selectedValueIds.length) {
        throw new ApiError(
          HTTP_STATUS.BAD_REQUEST,
          `Duplicated values are not allowed for option "${option.name}"`,
        );
      }

      const values = selectedValueIds.map((valueId) => {
        const value = option.values.find((candidate) => candidate.id === valueId);

        if (!value || !value.isAvailable) {
          throw new ApiError(
            HTTP_STATUS.BAD_REQUEST,
            `Selected value does not exist or is unavailable for option "${option.name}"`,
          );
        }

        extraPrice += value.additionalPrice;

        return {
          valueId: value.id,
          name: value.name,
          additionalPrice: value.additionalPrice,
        };
      });

      selectedOptions.push({
        optionId: option.id,
        name: option.name,
        values,
      });
    }

    const invalidOptionIds = selectedOptionsInput
      .map((option) => option.optionId)
      .filter((optionId) => !product.options.some((option) => option.id === optionId));

    if (invalidOptionIds.length > 0) {
      throw new ApiError(HTTP_STATUS.BAD_REQUEST, "Some selected options are invalid");
    }

    const baseUnitPrice = product.discountPrice ?? product.basePrice;

    return {
      note,
      selectedOptions: selectedOptions.sort((a, b) => a.optionId.localeCompare(b.optionId)),
      unitPrice: baseUnitPrice + extraPrice,
    };
  }

  private buildSignature(
    productId: string,
    selectedOptions: CartSelectedOptionDto[],
    note: string | null,
  ) {
    return JSON.stringify({
      productId,
      note,
      selectedOptions: selectedOptions.map((option) => ({
        optionId: option.optionId,
        values: option.values
          .map((value) => value.valueId)
          .sort((a, b) => a.localeCompare(b)),
      })),
    });
  }
}
