import { Router } from "express";
import categoryRoutes from "./category.route";

const router = Router();

router.use("/categories", categoryRoutes);

export default router;