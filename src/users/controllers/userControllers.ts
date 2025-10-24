import { Request, Response } from "express";
import { User } from "../models/userModels";
import {
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  GET_METHOD,
  INTERNAL_SERVER_ERROR,
  OK,
  USER_NOT_FOUND,
} from "../../config/constants";

// Récupérer les informations du profil
export const GetUserProfileController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== GET_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Récupération de l'utilisateur via requête client (la requête est de type `Request` de base + étendue avec `User`)
    const user: User = req.user;

    res.status(OK).send(user);
  } catch (error: unknown) {
    res.status(INTERNAL_SERVER_ERROR).json({
      message: USER_NOT_FOUND,
      error: error,
    });
  }
};
