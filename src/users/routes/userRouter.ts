import { Router } from "express";
const userRouter = Router();

import { GetUserProfileController } from "../controllers/userControllers.js";
import { authMiddleware } from "../../auth/middlewares/authMiddleware.js";

userRouter.get("/profile", authMiddleware, GetUserProfileController);

export default userRouter;
