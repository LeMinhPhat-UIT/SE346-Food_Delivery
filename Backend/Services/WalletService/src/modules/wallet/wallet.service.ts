import { WalletOwnerType } from "@prisma/client";
import { HTTP_STATUS } from "../../constants/httpStatus";
import { ApiError } from "../../utils/apiError";
import {
  ListTransactionsQueryDto,
  WalletResponseDto,
  WalletTransactionResponseDto,
  WalletWithTransactionsResponseDto,
} from "./wallet.dto";
import { toWalletResponseDto, toWalletTransactionResponseDto } from "./wallet.mapper";
import { WalletRepository } from "./wallet.repository";

export class WalletService {
  constructor(private readonly walletRepository: WalletRepository) {}

  async getMyWallet(ownerType: WalletOwnerType, ownerId: string): Promise<WalletResponseDto> {
    const wallet = await this.walletRepository.findByOwner(ownerType, ownerId);

    if (!wallet) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Wallet not found");
    }

    return toWalletResponseDto(wallet);
  }

  async getMyTransactions(
    ownerType: WalletOwnerType,
    ownerId: string,
    query: ListTransactionsQueryDto,
  ): Promise<WalletWithTransactionsResponseDto> {
    const { wallet, items, total } = await this.walletRepository.findTransactionsByOwner(
      ownerType,
      ownerId,
      query.page,
      query.limit,
    );

    if (!wallet) {
      throw new ApiError(HTTP_STATUS.NOT_FOUND, "Wallet not found");
    }

    return {
      wallet: toWalletResponseDto(wallet),
      transactions: items.map(toWalletTransactionResponseDto),
      meta: {
        page: query.page,
        limit: query.limit,
        total,
        totalPages: Math.max(1, Math.ceil(total / query.limit)),
      },
    };
  }

  async ensureWallet(ownerType: WalletOwnerType, ownerId: string): Promise<WalletResponseDto> {
    const wallet = await this.walletRepository.upsertWallet(ownerType, ownerId);
    return toWalletResponseDto(wallet);
  }
}
