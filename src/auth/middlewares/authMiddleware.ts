import { NextFunction, Request, Response } from "express";
import jwt, { Secret, JwtPayload } from "jsonwebtoken";
import { SECRET_KEY } from "../../config/variables";

let assignToken: string | JwtPayload;

export const authMiddleware = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    const token = req.header("Authorization")?.replace("Bearer ", "");

    if (!token) {
      throw new Error();
    }

    const decoded = jwt.verify(token, SECRET_KEY);
    assignToken = decoded;

    next();
  } catch (err) {
    res.status(401).send("Accès non autorisé");
  }
};
