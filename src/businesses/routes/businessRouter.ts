import { Router } from "express";
const businessRouter = Router();

import { GetBusinessController } from "../controllers/businessControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";

businessRouter.get("/:id", authMiddleware, GetBusinessController);

export default businessRouter;
