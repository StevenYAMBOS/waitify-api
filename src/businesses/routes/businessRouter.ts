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
import { ROUTES_BUSINESSES, ROUTES_USERS } from "../../config/constants";

businessRouter.get(
  ROUTES_BUSINESSES.BY_ID,
  authMiddleware,
  checkBusinessOwnership,
  GetBusinessController
);
businessRouter.get(
  ROUTES_USERS.BASE + ROUTES_BUSINESSES.BY_ID,
  authMiddleware,
  checkBusinessOwnership,
  GetUserBusinessesController
);
businessRouter.post(
  "/generate",
  authMiddleware,
  checkBusinessOwnership,
  GenerateQRCodeController
);
businessRouter.post(
  "/",
  authMiddleware,
  checkBusinessOwnership,
  AddBusinessController
);
businessRouter.patch(
  ROUTES_BUSINESSES.BY_ID,
  authMiddleware,
  checkBusinessOwnership,
  UpdateBusinessController
);
businessRouter.delete(
  ROUTES_BUSINESSES.BY_ID,
  authMiddleware,
  checkBusinessOwnership,
  DeleteBusinessController
);

export default businessRouter;
