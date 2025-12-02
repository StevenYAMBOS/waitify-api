import { Request, Response } from "express";
import jwt from "jsonwebtoken";
import { SECRET_KEY } from "../../config/envVariables";
import {
  AUTH,
  ERROR_MESSAGES,
  GOOGLE_API,
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
export const ProtectedController = async (res: Response) => {
  res.status(HTTP_STATUS.OK).json(`Accès à la route protégé !`);
};

export const GoogleOAuthPortalController = async (
  req: Request,
  res: Response
) => {
  const state = "randomstate";
  const scopes = GOOGLE_API.OAUTH_SCOPES.join(" ");
  const GOOGLE_OAUTH_CONSENT_SCREEN_URL = `${GOOGLE_API.OAUTH_URL}?client_id=${GOOGLE_API.CLIENT_ID}&redirect_uri=${GOOGLE_API.REDIRECT_URL}&access_type=offline&response_type=code&state=${state}&scope=${scopes}`;
  res.redirect(GOOGLE_OAUTH_CONSENT_SCREEN_URL);
};

export const GoogleOAuthCallbackController = async (
  req: Request,
  res: Response
) => {
  try {
    const { code } = req.query;
    if (!code) {
      console.error("Google OAuth: Code manquant dans la requête");
      return res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: ERROR_MESSAGES.INVALID_REQUEST,
      });
    }

    const data = {
      code,
      client_id: GOOGLE_API.CLIENT_ID,
      client_secret: GOOGLE_API.CLIENT_SECRET,
      redirect_uri: GOOGLE_API.REDIRECT_URL,
      grant_type: "authorization_code",
    };
    console.log("Google OAuth: Échange du code contre un token...", { data });

    // Échange du code contre un token
    const tokenResponse = await fetch(GOOGLE_API.ACCESS_TOKEN_URL, {
      method: HTTP_METHODS.POST,
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });

    if (!tokenResponse.ok) {
      console.error("Google OAuth: Échec de l'échange du code", {
        status: tokenResponse.status,
        data: await tokenResponse.text(),
      });
      return res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
        error: ERROR_MESSAGES.EXTERNAL_SERVICE_ERROR,
      });
    }

    const accessTokenData = await tokenResponse.json();
    const { id_token } = accessTokenData;
    console.log("Google OAuth: Token reçu", { id_token });

    // Récupération des infos utilisateur
    const userInfoResponse = await fetch(
      `${process.env.GOOGLE_TOKEN_INFO_URL}?id_token=${id_token}`
    );

    if (!userInfoResponse.ok) {
      console.error(
        "Google OAuth: Échec de la récupération des infos utilisateur",
        {
          status: userInfoResponse.status,
          data: await userInfoResponse.text(),
        }
      );
      return res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
        error: ERROR_MESSAGES.EXTERNAL_SERVICE_ERROR,
      });
    }

    const userInfo = await userInfoResponse.json();
    console.log("Google OAuth: Infos utilisateur récupérées", { userInfo });

    // Ici, tu peux créer un utilisateur ou un token JWT pour ton application
    const token = jwt.sign(
      { email: userInfo.email, sub: userInfo.sub },
      SECRET_KEY,
      { expiresIn: AUTH.EXPIRATION_TIME }
    );

    res.status(HTTP_STATUS.OK).json({ token, user: userInfo });
  } catch (error) {
    console.error("Google OAuth: Erreur inattendue", { error });
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      error: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
    });
  }
};
