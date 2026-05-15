import { Router } from "express";
import {
  attachMerchantContext,
  authenticate,
  requireRoles,
} from "../../middlewares/auth.middleware";
import { validate } from "../../middlewares/validate.middleware";
import { ROLES } from "../../constants/roles";
import { VoucherController } from "./voucher.controller";
import {
  createVoucherSchema,
  listVouchersSchema,
  restoreVoucherSchema,
  updateVoucherSchema,
  updateVoucherStatusSchema,
  validateVoucherSchema,
  voucherCodeSchema,
  voucherIdSchema,
} from "./voucher.schema";

const router = Router();
const voucherController = new VoucherController();

router.get("/", authenticate, validate(listVouchersSchema), voucherController.getAllVouchers);
router.get("/code/:code", authenticate, validate(voucherCodeSchema), voucherController.getVoucherByCode);
router.post(
  "/validate",
  authenticate,
  requireRoles(ROLES.CUSTOMER, ROLES.MERCHANT, ROLES.ADMIN),
  validate(validateVoucherSchema),
  voucherController.validateVoucher,
);
router.post(
  "/",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(createVoucherSchema),
  voucherController.createVoucher,
);
router.patch(
  "/:id/status",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateVoucherStatusSchema),
  voucherController.updateVoucherStatus,
);
router.patch(
  "/:id/restore",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(restoreVoucherSchema),
  voucherController.restoreVoucher,
);
router.get("/:id", authenticate, validate(voucherIdSchema), voucherController.getVoucherById);
router.put(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateVoucherSchema),
  voucherController.updateVoucher,
);
router.patch(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(updateVoucherSchema),
  voucherController.updateVoucher,
);
router.delete(
  "/:id",
  authenticate,
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate(voucherIdSchema),
  voucherController.deleteVoucher,
);

export default router;
