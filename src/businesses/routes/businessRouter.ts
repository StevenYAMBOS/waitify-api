import { Router } from "express";
const businessRouter = Router();

import {
  AddBusinessController,
  GetBusinessController,
  TestController,
  GetUserBusinessesController,
} from "../controllers/businessControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import {
  ID_PARAM,
  NEUTRAL_PATH,
  TEST_PATH,
  USER_PATH,
} from "../../config/constants";

businessRouter.get(ID_PARAM, authMiddleware, GetBusinessController);
businessRouter.get(
  USER_PATH + ID_PARAM,
  authMiddleware,
  GetUserBusinessesController
);
businessRouter.post(NEUTRAL_PATH, authMiddleware, AddBusinessController);
businessRouter.get(TEST_PATH, authMiddleware, TestController);

export default businessRouter;
