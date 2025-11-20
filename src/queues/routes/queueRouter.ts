import { Router } from "express";
const queueRouter = Router();

import {
  ActivateQueueController,
  GetQueueStatusController,
  JoinQueueController,
  CallNextClientController,
  CancelQueueEntryController,
  MarkClientAsServedController,
} from "../controllers/queueControllers";
import {
  authMiddleware,
  checkBusinessOwnership,
} from "../../auth/middlewares/authMiddleware";
import { ROUTES_BUSINESSES, ROUTES_QUEUES } from "../../config/constants";

queueRouter.patch(
  ROUTES_BUSINESSES.BASE + ROUTES_BUSINESSES.BY_ID + "/status",
  authMiddleware,
  checkBusinessOwnership,
  ActivateQueueController
);
queueRouter.patch(
  ROUTES_QUEUES.MARK_SERVED,
  authMiddleware,
  checkBusinessOwnership,
  MarkClientAsServedController
);
queueRouter.post(
  ROUTES_BUSINESSES.BASE + ROUTES_BUSINESSES.BY_ID + ROUTES_QUEUES.JOIN,
  authMiddleware,
  JoinQueueController
);
queueRouter.get(ROUTES_QUEUES.STATUS, authMiddleware, GetQueueStatusController);
queueRouter.post(
  ROUTES_QUEUES.NEXT_CLIENT,
  authMiddleware,
  checkBusinessOwnership,
  CallNextClientController
);
queueRouter.delete(
  ROUTES_QUEUES.CANCEL,
  authMiddleware,
  CancelQueueEntryController
);
export default queueRouter;
