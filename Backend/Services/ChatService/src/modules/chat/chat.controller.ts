import { Request, Response } from "express";
import { ChatConversationType } from "@prisma/client";
import { ChatService } from "./chat.service";
import { sendSuccess } from "../../utils/response";
import { HTTP_STATUS } from "../../constants/httpStatus";

export class ChatController {
  constructor(private readonly chatService = new ChatService()) {}

  private parsePagination(query: Request["query"]) {
    const page = Number(query.page ?? 1);
    const limit = Number(query.limit ?? 20);

    return {
      page: Number.isFinite(page) && page > 0 ? Math.floor(page) : 1,
      limit: Number.isFinite(limit) && limit > 0 ? Math.min(Math.floor(limit), 100) : 20,
    };
  }

  createConversation = async (req: Request, res: Response) => {
    const conversation = await this.chatService.createConversation(req.auth!, req.body);
    return sendSuccess(res, {
      message: "Conversation created successfully",
      statusCode: HTTP_STATUS.CREATED,
      data: conversation,
    });
  };

  getConversation = async (req: Request, res: Response) => {
    const conversationId = String(req.params.conversationId);
    const conversation = await this.chatService.getConversation(req.auth!, conversationId);
    return sendSuccess(res, {
      message: "Conversation fetched successfully",
      data: conversation,
    });
  };

  getConversationByOrder = async (req: Request, res: Response) => {
    const orderId = String(req.params.orderId);
    const conversationType = req.params.conversationType as ChatConversationType;
    const conversation = await this.chatService.getConversationByOrder(
      req.auth!,
      orderId,
      conversationType,
    );
    return sendSuccess(res, {
      message: "Conversation fetched successfully",
      data: conversation,
    });
  };

  listConversations = async (req: Request, res: Response) => {
    const result = await this.chatService.listConversations(req.auth!, {
      ...this.parsePagination(req.query),
      conversationType: req.query.conversationType as never,
    });
    return sendSuccess(res, {
      message: "Conversations fetched successfully",
      data: result,
    });
  };

  getMessages = async (req: Request, res: Response) => {
    const conversationId = String(req.params.conversationId);
    const result = await this.chatService.getMessages(
      req.auth!,
      conversationId,
      this.parsePagination(req.query),
    );
    return sendSuccess(res, {
      message: "Messages fetched successfully",
      data: result,
    });
  };

  sendMessage = async (req: Request, res: Response) => {
    const conversationId = String(req.params.conversationId);
    const message = await this.chatService.sendMessage(
      req.auth!,
      conversationId,
      req.body,
    );
    return sendSuccess(res, {
      message: "Message sent successfully",
      statusCode: HTTP_STATUS.CREATED,
      data: message,
    });
  };

  markConversationRead = async (req: Request, res: Response) => {
    const conversationId = String(req.params.conversationId);
    const conversation = await this.chatService.markConversationAsRead(
      req.auth!,
      conversationId,
    );
    return sendSuccess(res, {
      message: "Conversation marked as read successfully",
      data: conversation,
    });
  };
}
