import { WalletController } from "./wallet.controller";
import { WalletRepository } from "./wallet.repository";
import { WalletService } from "./wallet.service";

export const walletRepository = new WalletRepository();
export const walletService = new WalletService(walletRepository);
export const walletController = new WalletController();
