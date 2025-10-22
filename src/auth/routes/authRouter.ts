import { Router } from "express";
const authRouter = Router();

import {
  LoginController,
  ProtectedController,
  RegisterController,
} from "../controllers/authControllers.js";
import { authMiddleware } from "../middlewares/authMiddleware.js";

authRouter.post("/register", RegisterController);
authRouter.post("/login", LoginController);
authRouter.get("/protected", authMiddleware, ProtectedController);

export default authRouter;
