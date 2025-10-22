import { Router } from "express";
const authRouter = Router();

import {
  LoginController,
  RegisterController,
} from "../controllers/authControllers.js";

authRouter.post("/register", RegisterController);
authRouter.post("/login", LoginController);

export default authRouter;
