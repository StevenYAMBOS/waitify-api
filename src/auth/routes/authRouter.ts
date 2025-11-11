import { Router } from "express";
const authRouter = Router();

import {
  LoginController,
  ProtectedController,
  RegisterController,
} from "../controllers/authControllers";
// import { authMiddleware } from "../middlewares/authMiddleware";
import {
  LOGIN_PATH,
  PROTECTED_PATH,
  REGISTER_PATH,
} from "../../config/constants";

authRouter.post(REGISTER_PATH, RegisterController);
authRouter.post(LOGIN_PATH, LoginController);
authRouter.get(PROTECTED_PATH, ProtectedController);

export default authRouter;
