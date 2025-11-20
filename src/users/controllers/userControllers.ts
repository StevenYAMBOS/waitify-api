import { Request, Response } from "express";
import { User } from "../models/userModels";
import {
  ERROR_MESSAGES,
  HTTP_METHODS,
  HTTP_STATUS,
  USER_MESSAGES,
} from "../../config/constants";

// Récupérer les informations du profil
export const GetUserProfileController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.GET) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    // Récupération de l'utilisateur via requête client (la requête est de type `Request` de base + étendue avec `User`)
    const user: User = req.user;

    res.status(HTTP_STATUS.OK).send(user);
  } catch (error: unknown) {
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: USER_MESSAGES.NOT_FOUND,
      error: error,
    });
  }
};
