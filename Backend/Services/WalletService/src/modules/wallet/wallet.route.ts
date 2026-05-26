import { Router } from "express";
import { ROLES } from "../../constants/roles";
import { authenticate, requireRoles } from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { walletController } from "./wallet.bootstrap";
import {
  topupBodySchema,
  topupParamSchema,
  listTransactionsQuerySchema,
  negativeWalletsQuerySchema,
  orderTransactionParamSchema,
  ownerParamSchema,
  referenceTransactionParamSchema,
} from "./wallet.schema";

const router = Router();

router.get("/health", (_req, res) => {
  res.status(200).send("Wallet Service is running healthy!");
});

router.get("/topup/vnpay/return", walletController.handleTopupVnpayReturn);
router.get("/topup/vnpay/ipn", walletController.handleTopupVnpayIpn);

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
  "/me/transactions/order/:orderId",
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ params: orderTransactionParamSchema, query: listTransactionsQuerySchema }),
  walletController.getMyTransactionsByOrderId,
);

router.post(
  "/me/topup/vnpay/url",
  requireRoles(ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ body: topupBodySchema }),
  walletController.createTopupVnpayPaymentUrl,
);

router.get(
  "/me/topups",
  requireRoles(ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ query: listTransactionsQuerySchema }),
  walletController.getMyTopups,
);

router.get(
  "/me/topups/:topupId",
  requireRoles(ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ params: topupParamSchema }),
  walletController.getMyTopupById,
);

router.get(
  "/me/transactions/reference/:referenceType/:referenceId",
  requireRoles(ROLES.ADMIN, ROLES.MERCHANT, ROLES.SHIPPER),
  validate({ params: referenceTransactionParamSchema, query: listTransactionsQuerySchema }),
  walletController.getMyTransactionsByReference,
);

router.get(
  "/admin/owners/:ownerType/:ownerId",
  requireRoles(ROLES.ADMIN),
  validate({ params: ownerParamSchema }),
  walletController.getWalletByOwner,
);

router.get(
  "/admin/negative",
  requireRoles(ROLES.ADMIN),
  validate({ query: negativeWalletsQuerySchema }),
  walletController.getNegativeWallets,
);

export default router;
