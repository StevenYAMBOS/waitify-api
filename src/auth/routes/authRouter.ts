import { Router } from "express";
const authRouter = Router();

import { Register } from "../controllers/authControllers.js";

authRouter.post("/register", Register);

export default authRouter;
