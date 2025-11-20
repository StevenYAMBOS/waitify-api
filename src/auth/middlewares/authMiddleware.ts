import { NextFunction, Request, Response } from "express";
import jwt from "jsonwebtoken";
import { SECRET_KEY } from "../../config/envVariables";
import { User } from "../../users/models/userModels";
import { AUTH, HTTP_STATUS } from "../../config/constants";
import { pool } from "../../config/database";

export const authMiddleware = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    const token = req.header(AUTH.HEADER_NAME)?.replace(AUTH.BEARER_PREFIX, "");

    if (!token) {
      throw new Error();
    }

    jwt.verify(token, SECRET_KEY, (err: unknown, user: User) => {
      if (err)
        return res.status(HTTP_STATUS.FORBIDDEN).send(AUTH.INVALID_TOKEN);
      req.user = user;
      next();
    });
  } catch (err: unknown) {
    console.error("Erreur authentification : ", err);
    res.status(HTTP_STATUS.UNAUTHORIZED).send(AUTH.UNAUTHORIZED_ACCESS);
  }
};

// Vérifier que le commerce appartient à l'utilisateur
export const checkBusinessOwnership = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  const { businessId } = req.params;
  const userId = req.user.id;

  const query: string = `SELECT EXISTS(SELECT 1 FROM businesses WHERE id = $1 AND UserId = $2)`;
  const values: string[] = [businessId, userId];

  const result = await pool.query(query, values);

  if (!result.rows[0].exists) {
    return res
      .status(HTTP_STATUS.UNAUTHORIZED)
      .json({ error: AUTH.UNAUTHORIZED_ACCESS });
  }
  next();
};
