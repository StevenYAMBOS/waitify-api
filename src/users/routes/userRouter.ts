import { Router } from "express";
const userRouter = Router();

import { GetUserProfileController } from "../controllers/userControllers.js";
import { authMiddleware } from "../../auth/middlewares/authMiddleware.js";
import { PROFILE_PATH } from "../../config/constants.js";

userRouter.get(PROFILE_PATH, authMiddleware, GetUserProfileController);

export default userRouter;
