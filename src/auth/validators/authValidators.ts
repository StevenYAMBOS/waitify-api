import { ValidationError } from "../models/authModels";

export const validateEmail = (email: string): ValidationError | null => {
  if (
    !email ||
    email.length === 0 ||
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
  ) {
    return {
      champs: "Email",
      message: "L'adresse email est invalide",
    };
  }
  return null;
};

export const validatePassword = (password: string): ValidationError | null => {
  if (!password || password.length === 0) {
    return {
      champs: "Mot de passe",
      message: "Le mot de passe est invalide",
    };
  }
  return null;
};
