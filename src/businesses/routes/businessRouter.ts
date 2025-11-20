import { Router } from "express";
const businessRouter = Router();

import {
  AddBusinessController,
  GetBusinessController,
  GenerateQRCodeController,
  GetUserBusinessesController,
  UpdateBusinessController,
  DeleteBusinessController,
} from "../controllers/businessControllers";
import {
  authMiddleware,
  checkBusinessOwnership,
} from "../../auth/middlewares/authMiddleware";
import {
  ID_PARAM,
  NEUTRAL_PATH,
  QRCODE_PATH,
  USER_PATH,
} from "../../config/constants";

businessRouter.get(
  ID_PARAM,
  authMiddleware,
  checkBusinessOwnership,
  GetBusinessController
);
businessRouter.get(
  USER_PATH + ID_PARAM,
  authMiddleware,
  checkBusinessOwnership,
  GetUserBusinessesController
);
businessRouter.post(
  QRCODE_PATH,
  authMiddleware,
  checkBusinessOwnership,
  GenerateQRCodeController
);
businessRouter.post(
  NEUTRAL_PATH,
  authMiddleware,
  checkBusinessOwnership,
  AddBusinessController
);
businessRouter.patch(
  ID_PARAM,
  authMiddleware,
  checkBusinessOwnership,
  UpdateBusinessController
);
businessRouter.delete(
  ID_PARAM,
  authMiddleware,
  checkBusinessOwnership,
  DeleteBusinessController
);

export default businessRouter;
