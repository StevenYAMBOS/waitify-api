import { Router } from "express";
const businessRouter = Router();

import {
  AddBusinessController,
  GetBusinessController,
  TestController,
} from "../controllers/businessControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import { ID_PARAM } from "../../config/constants";

// businessRouter.get(ID_PARAM, authMiddleware, GetBusinessController);
businessRouter.get("/generateQR", authMiddleware, TestController);
businessRouter.post("/", authMiddleware, AddBusinessController);

export default businessRouter;
