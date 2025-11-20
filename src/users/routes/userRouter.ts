import { Router } from "express";
const userRouter = Router();

import { GetUserProfileController } from "../controllers/userControllers";
import { authMiddleware } from "../../auth/middlewares/authMiddleware";
import { ROUTES_USERS } from "../../config/constants";

userRouter.get(ROUTES_USERS.PROFILE, authMiddleware, GetUserProfileController);

export default userRouter;
