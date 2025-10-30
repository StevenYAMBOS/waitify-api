import { Router } from "express";
const userRouter = Router();

import { GetUserProfileController } from "../controllers/userControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import { PROFILE_PATH } from "../../config/constants";

userRouter.get(PROFILE_PATH, authMiddleware, GetUserProfileController);

export default userRouter;
