import { Router } from "express";
const authRouter = Router();

import {
  LoginController,
  ProtectedController,
  RegisterController,
} from "../controllers/authControllers.js";
import { authMiddleware } from "../middlewares/authMiddleware.js";
import {
  LOGIN_PATH,
  PROTECTED_PATH,
  REGISTER_PATH,
} from "../../config/constants.js";

authRouter.post(REGISTER_PATH, RegisterController);
authRouter.post(LOGIN_PATH, LoginController);
authRouter.get(PROTECTED_PATH, authMiddleware, ProtectedController);

export default authRouter;
