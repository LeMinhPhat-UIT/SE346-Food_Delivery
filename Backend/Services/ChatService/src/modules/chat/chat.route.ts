import { Router } from "express";
import { ChatController } from "./chat.controller";
import { authenticate } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import {
  chatFilterSchema,
  conversationOrderParamSchema,
  conversationIdParamSchema,
  createConversationSchema,
  createMessageSchema,
  paginationSchema,
} from "./chat.schema";

const router = Router();
const chatController = new ChatController();

router.use(authenticate);

router.get("/conversations", validate(chatFilterSchema, "query"), chatController.listConversations);
router.post("/conversations", validate(createConversationSchema), chatController.createConversation);
router.get(
  "/orders/:orderId/:conversationType",
  validate(conversationOrderParamSchema, "params"),
  chatController.getConversationByOrder,
);
router.get(
  "/conversations/:conversationId",
  validate(conversationIdParamSchema, "params"),
  chatController.getConversation,
);
router.get(
  "/conversations/:conversationId/messages",
  validate(conversationIdParamSchema, "params"),
  validate(paginationSchema, "query"),
  chatController.getMessages,
);
router.post(
  "/conversations/:conversationId/messages",
  validate(conversationIdParamSchema, "params"),
  validate(createMessageSchema),
  chatController.sendMessage,
);
router.patch(
  "/conversations/:conversationId/read",
  validate(conversationIdParamSchema, "params"),
  chatController.markConversationRead,
);

export default router;
