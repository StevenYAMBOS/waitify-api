import { Router } from "express";
const businessRouter = Router();

import { GetBusinessController } from "../controllers/businessControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import { ID_PARAM } from "../../config/constants";

businessRouter.get(ID_PARAM, authMiddleware, GetBusinessController);

export default businessRouter;
