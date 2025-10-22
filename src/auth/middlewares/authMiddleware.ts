import { NextFunction, Request, Response } from "express";
import jwt, { Secret, JwtPayload } from "jsonwebtoken";
import { SECRET_KEY } from "../../config/variables";
import { User } from "../../users/models/userModels";

// let assignToken: string | JwtPayload;

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

    jwt.verify(token, SECRET_KEY, (err: unknown, user: User) => {
      if (err) return res.status(403).send("Invalid or expired token");
      req.user = user;
      next();
    });

    // const decoded = jwt.verify(token, SECRET_KEY);
    // assignToken = decoded;

    // next();
  } catch (err) {
    res.status(401).send("Accès non autorisé");
  }
};
