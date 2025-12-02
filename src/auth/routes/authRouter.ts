import { Router } from "express";
const authRouter = Router();

import {
  GoogleOAuthCallbackController,
  GoogleOAuthPortalController,
  LoginController,
  ProtectedController,
  RegisterController,
} from "../controllers/authControllers";
import { ROUTES_AUTH } from "../../config/constants";

authRouter.post(ROUTES_AUTH.REGISTER, RegisterController);
authRouter.post(ROUTES_AUTH.LOGIN, LoginController);
authRouter.get(ROUTES_AUTH.PROTECTED, ProtectedController);
authRouter.get(
  ROUTES_AUTH.GOOGLE + ROUTES_AUTH.LOGIN,
  GoogleOAuthPortalController
);
authRouter.get(
  ROUTES_AUTH.GOOGLE + ROUTES_AUTH.CALLBACK,
  GoogleOAuthCallbackController
);

export default authRouter;
