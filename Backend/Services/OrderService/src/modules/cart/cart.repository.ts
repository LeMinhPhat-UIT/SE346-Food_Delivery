import { env } from "../../config/env.config";
import { redis } from "../../config/redis.config";
import { CartStoredRecord } from "./cart.dto";

export class CartRepository {
  private getCartKey(userId: string, merchantId: string) {
    return `order:cart:${userId}:${merchantId}`;
  }

  async findByUserAndMerchantId(
    userId: string,
    merchantId: string,
  ): Promise<CartStoredRecord | null> {
    const rawCart = await redis.get(this.getCartKey(userId, merchantId));

    if (!rawCart) {
      return null;
    }

    return JSON.parse(rawCart) as CartStoredRecord;
  }

  async findAllByUserId(userId: string): Promise<CartStoredRecord[]> {
    const pattern = this.getCartKey(userId, "*");
    const carts: CartStoredRecord[] = [];
    const keys = await redis.keys(pattern);

    for (const key of keys) {
      const rawCart = await redis.get(key);

      if (rawCart) {
        carts.push(JSON.parse(rawCart) as CartStoredRecord);
      }
    }

    return carts.sort((a, b) => b.updatedAt.localeCompare(a.updatedAt));
  }

  async save(cart: CartStoredRecord): Promise<void> {
    await redis.set(this.getCartKey(cart.userId, cart.merchantId), JSON.stringify(cart), {
      EX: env.CART_TTL_SECONDS,
    });
  }

  async clearByUserAndMerchantId(userId: string, merchantId: string): Promise<void> {
    await redis.del(this.getCartKey(userId, merchantId));
  }

  async clearAllByUserId(userId: string): Promise<void> {
    const carts = await this.findAllByUserId(userId);

    if (carts.length === 0) {
      return;
    }

    const keys = carts.map((cart) => this.getCartKey(cart.userId, cart.merchantId));

    for (const key of keys) {
      await redis.del(key);
    }
  }
}
