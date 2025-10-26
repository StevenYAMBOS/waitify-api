import { Router } from "express";
const businessRouter = Router();

import {
  AddBusinessController,
  GetBusinessController,
  GenerateQRCodeController,
  GetUserBusinessesController,
} from "../controllers/businessControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import {
  ID_PARAM,
  NEUTRAL_PATH,
  QRCODE_PATH,
  QRCODE_TOKEN_PATH,
  TEST_PATH,
  USER_PATH,
} from "../../config/constants";

businessRouter.get(ID_PARAM, authMiddleware, GetBusinessController);
businessRouter.get(
  USER_PATH + ID_PARAM,
  authMiddleware,
  GetUserBusinessesController
);
businessRouter.post(QRCODE_PATH, authMiddleware, GenerateQRCodeController);
businessRouter.post(NEUTRAL_PATH, authMiddleware, AddBusinessController);

export default businessRouter;
