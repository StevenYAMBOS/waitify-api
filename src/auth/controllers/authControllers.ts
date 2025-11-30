import { Request, Response } from "express";
import jwt from "jsonwebtoken";
import { SECRET_KEY } from "../../config/envVariables";
import {
  AUTH,
  ERROR_MESSAGES,
  HTTP_METHODS,
  HTTP_STATUS,
  USER_MESSAGES,
} from "../../config/constants";
import {
  LoginUserService,
  RegisterUserService,
} from "../services/authServices";

export const RegisterController = async (req: Request, res: Response) => {
  if (req.method !== HTTP_METHODS.POST) {
    return res
      .status(HTTP_STATUS.BAD_REQUEST)
      .send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    const { email, password } = req.body;

    const result = await RegisterUserService({
      email,
      password,
    });

    if (result.errors) {
      return res.status(HTTP_STATUS.OK).json({
        errors: result.errors,
      });
    }

    res.status(HTTP_STATUS.CREATED).json({
      message: USER_MESSAGES.CREATION_SUCCESS,
      user: result.user,
    });
  } catch (error: unknown) {
    console.error("Erreur lors de l'inscription:", error);
    res
      .status(HTTP_STATUS.INTERNAL_SERVER_ERROR)
      .json({ error: ERROR_MESSAGES.INTERNAL_SERVER_ERROR });
  }
};

export const LoginController = async (req: Request, res: Response) => {
  if (req.method !== HTTP_METHODS.POST) {
    return res
      .status(HTTP_STATUS.BAD_REQUEST)
      .send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    const { email, password } = req.body;

    const result = await LoginUserService({ email, password });

    if (result.error) {
      return res.status(HTTP_STATUS.UNAUTHORIZED).json({
        status: ERROR_MESSAGES.INVALID_REQUEST,
        message: result.error,
        statusCode: HTTP_STATUS.UNAUTHORIZED,
      });
    }

    const token = jwt.sign({ user: result.user }, SECRET_KEY, {
      expiresIn: AUTH.EXPIRATION_TIME,
    });

    res.status(HTTP_STATUS.OK).json({
      message: USER_MESSAGES.LOGIN_SUCCESS,
      token: token,
      user: result.user,
    });
  } catch (error: unknown) {
    console.error("Erreur lors de la connexion:", error);
    res
      .status(HTTP_STATUS.INTERNAL_SERVER_ERROR)
      .json({ error: ERROR_MESSAGES.INTERNAL_SERVER_ERROR });
  }
};

// Test (route protégée)
export const ProtectedController = async (req: Request, res: Response) => {
  res.status(HTTP_STATUS.OK).json(`Accès à la route protégé !`);
};
