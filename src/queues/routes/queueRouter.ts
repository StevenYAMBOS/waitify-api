import { Router } from "express";
const queueRouter = Router();

import {
  ActivateQueueController,
  GetQueueStatusController,
  JoinQueueController,
} from "../controllers/queueControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import {
  BUSINESS_PATH,
  ID_PARAM,
  JOIN_QUEUE_PATH,
  QUEUE_STATUS_PATH,
} from "../../config/constants";

queueRouter.patch(
  BUSINESS_PATH + ID_PARAM + QUEUE_STATUS_PATH,
  authMiddleware,
  ActivateQueueController
);
queueRouter.post(
  BUSINESS_PATH + ID_PARAM + JOIN_QUEUE_PATH,
  authMiddleware,
  JoinQueueController
);
queueRouter.get(
  QUEUE_STATUS_PATH + ID_PARAM,
  authMiddleware,
  GetQueueStatusController
);
export default queueRouter;
