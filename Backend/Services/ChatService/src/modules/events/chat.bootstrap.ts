import { AssignmentAcceptedConsumer } from "./assignment-accepted.consumer";
import { OrderCompletedConsumer } from "./order-completed.consumer";

export const orderCompletedConsumer = new OrderCompletedConsumer();
export const assignmentAcceptedConsumer = new AssignmentAcceptedConsumer();
