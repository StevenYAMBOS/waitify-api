import { Router } from "express";
const queueRouter = Router();

import { ActivateQueueHandler } from "../controllers/queueControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import { BUSINESS_PATH, ID_PARAM } from "../../config/constants";

queueRouter.get(BUSINESS_PATH + ID_PARAM, authMiddleware, ActivateQueueHandler);

export default queueRouter;
