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
import { UserRepository } from "../../users/repositories/userRepositories";
import { GoogleAuthService } from "../services/googleAuthServices";
import crypto from "crypto";

const userRepository = new UserRepository();
const googleAuthService = new GoogleAuthService(userRepository);
const oauthStates = new Map<string, { timestamp: number }>();

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
export const ProtectedController = async (res: Response) => {
  res.status(HTTP_STATUS.OK).json(`Accès à la route protégé !`);
};

export const GoogleOAuthPortalController = async (
  req: Request,
  res: Response
) => {
  try {
    // Générer un state aléatoire sécurisé
    const state = crypto.randomBytes(32).toString("hex");

    // Stocker le state (expire après 10 minutes)
    oauthStates.set(state, { timestamp: Date.now() });

    // Nettoyer les anciens states (plus de 10 minutes)
    const tenMinutesAgo = Date.now() - 10 * 60 * 1000;
    for (const [key, value] of oauthStates.entries()) {
      if (value.timestamp < tenMinutesAgo) {
        oauthStates.delete(key);
      }
    }

    const consentUrl = googleAuthService.getConsentScreenUrl(state);
    console.log("Redirection vers Google OAuth", {
      url: consentUrl,
    });

    res.redirect(consentUrl);
  } catch (error) {
    console.error("Erreur lors de l'init OAuth", {
      error: error instanceof Error ? error.message : error,
      stack: error instanceof Error ? error.stack : undefined,
    });

    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      error: ERROR_MESSAGES.EXTERNAL_SERVICE_ERROR,
    });
  }
};

export const GoogleOAuthCallbackController = async (
  req: Request,
  res: Response
) => {
  try {
    console.log("Callback Google reçu", {
      query: req.query,
    });

    const { code, state, error } = req.query;

    if (error) {
      console.error("Erreur retournée par Google", { error });
      return res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: ERROR_MESSAGES.GOOGLE_AUTH_FAILED,
        details: error,
      });
    }

    if (!state || typeof state !== "string" || !oauthStates.has(state)) {
      console.error("State invalide ou manquant", { state });
      return res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: ERROR_MESSAGES.INVALID_SESSION,
      });
    }

    oauthStates.delete(state);

    if (!code || typeof code !== "string") {
      console.error("Code d'autorisation manquant");
      return res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: ERROR_MESSAGES.INVALID_CODE,
      });
    }

    const tokens = await googleAuthService.exchangeCodeForTokens(code);
    if (!tokens) {
      console.error("Échec de l'échange du code");
      return res.status(HTTP_STATUS.UNAUTHORIZED).json({
        error: ERROR_MESSAGES.EXCHANGE_CODE_FAILED,
      });
    }

    const googleUser = await googleAuthService.getUserInfo(tokens.id_token);
    if (!googleUser) {
      console.error("Échec de la récupération des infos");
      return res.status(HTTP_STATUS.UNAUTHORIZED).json({
        error: USER_MESSAGES.FAILED_FETCH,
      });
    }

    if (!googleUser.email_verified) {
      console.error("Email non vérifié", {
        email: googleUser.email,
      });
      return res.status(HTTP_STATUS.FORBIDDEN).json({
        error: USER_MESSAGES.UNAUTHORIZED,
      });
    }

    const user = await googleAuthService.findOrCreateUser(googleUser);

    const token = jwt.sign(
      {
        user: {
          id: user.id,
          email: user.email,
        },
      },
      SECRET_KEY,
      { expiresIn: "7d" }
    );

    res.status(HTTP_STATUS.OK).json({
      message: USER_MESSAGES.LOGIN_SUCCESS,
      token,
      user: {
        id: user.id,
        email: user.email,
        profile_picture: user.profilePicture,
      },
    });
  } catch (error) {
    console.error("Erreur lors du callback OAuth", {
      error: error instanceof Error ? error.message : error,
      stack: error instanceof Error ? error.stack : undefined,
    });

    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      error: ERROR_MESSAGES.EXTERNAL_SERVICE_ERROR,
    });
  }
};
