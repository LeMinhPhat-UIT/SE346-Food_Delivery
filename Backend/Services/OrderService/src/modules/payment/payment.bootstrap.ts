import { OrderRepository } from "../order/order.repository";
import { PaymentController } from "./payment.controller";
import { PaymentRepository } from "./payment.repository";
import { PaymentService } from "./payment.service";

export const paymentRepository = new PaymentRepository();
export const orderRepository = new OrderRepository();
export const paymentService = new PaymentService(paymentRepository, orderRepository);
export const paymentController = new PaymentController();

