import { Router } from "express";
import {
  attachMerchantContext,
  attachShipperContext,
  authenticate,
  requireRoles,
} from "../../middlewares/auth.middleware";
import { ROLES } from "../../constants/roles";
import { validate } from "../../middlewares/validate.middleware";
import { reportController } from "./report.bootstrap";
import { dateRangeQuerySchema } from "./report.schema";

const router = Router();

router.get("/health", (_req, res) => {
  res.status(200).send("Report Service is running healthy!");
});

router.use(authenticate);

router.get(
  "/admin/overview",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getAdminOverview,
);

router.get(
  "/admin/top-merchants",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopMerchants,
);

router.get(
  "/admin/top-shippers",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopShippers,
);

router.get(
  "/admin/top-products",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopProducts,
);

router.get(
  "/admin/top-merchants",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopMerchants,
);

router.get(
  "/admin/top-shippers",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopShippers,
);

router.get(
  "/admin/top-products",
  requireRoles(ROLES.ADMIN),
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopProducts,
);

router.get(
  "/merchant/me/overview",
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate({ query: dateRangeQuerySchema }),
  reportController.getMerchantOverview,
);

router.get(
  "/merchant/me/top-products",
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopProducts,
);

router.get(
  "/merchant/me/top-products",
  requireRoles(ROLES.MERCHANT, ROLES.ADMIN),
  attachMerchantContext,
  validate({ query: dateRangeQuerySchema }),
  reportController.getTopProducts,
);

router.get(
  "/shipper/me/overview",
  requireRoles(ROLES.SHIPPER, ROLES.ADMIN),
  attachShipperContext,
  validate({ query: dateRangeQuerySchema }),
  reportController.getShipperOverview,
);

export default router;
