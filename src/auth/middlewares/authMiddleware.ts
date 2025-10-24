import { NextFunction, Request, Response } from "express";
import jwt from "jsonwebtoken";
import { SECRET_KEY } from "../../config/envVariables";
import { User } from "../../users/models/userModels";
import {
  AUTHORIZATION_HEADER,
  BEARER_STRING,
  EMPTY_STRING,
  FORBIDDEN,
  INVALID_TOKEN,
  UNAUTHORIZED,
  UNAUTHORIZED_RESOURCE,
} from "../../config/constants";

export const authMiddleware = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    const token = req
      .header(AUTHORIZATION_HEADER)
      ?.replace(BEARER_STRING, EMPTY_STRING);

    if (!token) {
      throw new Error();
    }

    jwt.verify(token, SECRET_KEY, (err: unknown, user: User) => {
      if (err) return res.status(FORBIDDEN).send(INVALID_TOKEN);
      req.user = user;
      next();
    });
  } catch (err) {
    res.status(UNAUTHORIZED).send(UNAUTHORIZED_RESOURCE);
  }
};
