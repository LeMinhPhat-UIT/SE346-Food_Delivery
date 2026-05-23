import { CatalogServiceClient } from "../../integrations/catalog.service";
import { UserServiceClient } from "../../integrations/user.service";
import { CartRepository } from "../cart/cart.repository";
import { CartService } from "../cart/cart.service";
import { OrderRepository } from "./order.repository";
import { OrderService } from "./order.service";

export const orderRepository = new OrderRepository();
export const cartRepository = new CartRepository();
export const catalogServiceClient = new CatalogServiceClient();
export const cartService = new CartService(cartRepository, catalogServiceClient);
export const userServiceClient = new UserServiceClient();
export const orderService = new OrderService(
  orderRepository,
  cartService,
  userServiceClient,
);
