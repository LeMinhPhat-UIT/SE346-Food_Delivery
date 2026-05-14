import { Router } from "express";
import { validate } from "../../middlewares/validate.middleware";
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

router.get("/", validate(listVouchersSchema), voucherController.getAllVouchers);
router.get("/code/:code", validate(voucherCodeSchema), voucherController.getVoucherByCode);
router.post("/validate", validate(validateVoucherSchema), voucherController.validateVoucher);
router.post("/", validate(createVoucherSchema), voucherController.createVoucher);
router.patch(
  "/:id/status",
  validate(updateVoucherStatusSchema),
  voucherController.updateVoucherStatus,
);
router.patch(
  "/:id/restore",
  validate(restoreVoucherSchema),
  voucherController.restoreVoucher,
);
router.get("/:id", validate(voucherIdSchema), voucherController.getVoucherById);
router.put("/:id", validate(updateVoucherSchema), voucherController.updateVoucher);
router.patch("/:id", validate(updateVoucherSchema), voucherController.updateVoucher);
router.delete("/:id", validate(voucherIdSchema), voucherController.deleteVoucher);

export default router;
