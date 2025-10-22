import { Request, Response } from "express";
import { User } from "../models/userModels";

// Récupérer les informations du profil
export const GetUserProfileController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== "GET") {
    res.status(400).send("Mauvaise méthode HTTP.");
  }

  try {
    // Récupération de l'utilisateur via requête client (la requête est de type `Request` de base + étendue avec `User`)
    const response: User = req.user;
    console.log("RÉPONSE : ", response);

    res.status(200).send(response);
  } catch (error: unknown) {
    res.status(500).json({
      message: `Une erreur est survenue lors de la récupération des informations de l'utilisateur`,
      error: error,
    });
  }
};
