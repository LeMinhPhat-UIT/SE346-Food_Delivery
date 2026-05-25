import { Router } from "express";
import { ROLES } from "../../constants/roles";
import { authenticate, requireRoles } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { walletController } from "./wallet.bootstrap";
import { listTransactionsQuerySchema, ownerParamSchema } from "./wallet.schema";

const router = Router();

router.get("/health", (_req, res) => {
  res.status(200).send("Wallet Service is running healthy!");
});

router.use(authenticate);

router.get(
  "/me",
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.SHIPPER),
  walletController.getMyWallet,
);

router.get(
  "/me/transactions",
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ query: listTransactionsQuerySchema }),
  walletController.getMyTransactions,
);

router.get(
  "/admin/owners/:ownerType/:ownerId",
  requireRoles(ROLES.ADMIN),
  validate({ params: ownerParamSchema }),
  walletController.getWalletByOwner,
);

export default router;
