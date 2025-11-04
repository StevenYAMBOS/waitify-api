import { Router } from "express";
const queueRouter = Router();

import {
  ActivateQueueController,
  JoinQueueController,
} from "../controllers/queueControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import {
  BUSINESS_PATH,
  ID_PARAM,
  JOIN_QUEUE_PATH,
} from "../../config/constants";

queueRouter.get(
  BUSINESS_PATH + ID_PARAM,
  authMiddleware,
  ActivateQueueController
);
queueRouter.post(
  BUSINESS_PATH + ID_PARAM + JOIN_QUEUE_PATH,
  authMiddleware,
  JoinQueueController
);
export default queueRouter;
