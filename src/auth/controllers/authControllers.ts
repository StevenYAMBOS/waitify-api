import { Request, Response } from "express";
import { pool } from "../../config/database";
import { v4 as uuidv4 } from "uuid";
import jwt from "jsonwebtoken";
import bcrypt from "bcryptjs";
import { LoginResponse } from "../models/authModels";
import { User } from "../../users/models/userModels";
import { SECRET_KEY } from "../../config/envVariables";
import {
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  CREATED,
  IMAGE,
  INTERNAL_SERVER_ERROR,
  INTERNAL_SERVER_ERROR_MESSAGE,
  OK,
  POST_METHOD,
  UNAUTHORIZED,
} from "../../config/constants";

// Inscription
export const RegisterController = async (req: Request, res: Response) => {
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    const { email, password, profile_picture } = req.body;
    // Générer l'id
    const uuid = uuidv4();
    // Date du jour
    const date = new Date();
    // Hash password
    const hashedPassword = await bcrypt.hash(password, 10);
    // Insetion en base de données
    const query: string = `INSERT INTO users (id, email, password, profile_picture, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6)`;

    /* ------ Restrictions ------ */

    let errors = [];

    if (
      !email ||
      email.length == 0 ||
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
    ) {
      errors.push({
        champs: "Email",
        message: "L'adresse email est invalide",
      });
    }

    if (!password || password.length == 0) {
      errors.push({
        champs: "Mot de passe",
        message: "Le mot de passe est invalide",
      });
    }

    if (errors.length > 0) {
      return res.status(422).json({
        errors: errors,
      });
    }

    await pool.query(query, [
      uuid,
      email,
      hashedPassword,
      profile_picture || IMAGE,
      date,
      date,
    ]);

    const user = {
      id: uuid,
      email: email,
      profile_picture: profile_picture,
      createdAt: date,
    };

    const message: string = "Utilisateur créé avec succès";

    res.status(CREATED).json({ message, user });
  } catch (error: unknown) {
    res
      .status(INTERNAL_SERVER_ERROR)
      .json({ error: INTERNAL_SERVER_ERROR_MESSAGE });
  }
};

// Connexion
export const LoginController = async (req: Request, res: Response) => {
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    const { email, password } = req.body;
    // Date du jour
    const loginDate = new Date();
    // Query connexion
    const loginQuery: string = `SELECT * FROM users WHERE email=$1`;
    // Query MAJ date de connexion
    const updateDateQuery: string = `UPDATE users SET last_login = $1 WHERE email=$2`;

    if (
      !email ||
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) ||
      !password ||
      password.length == 0
    ) {
      return res.status(UNAUTHORIZED).json({
        status: "Mauvaise requête",
        message: "Erreur lors de la connexion",
        statusCode: 401,
      });
    }

    // Requête de connexion
    const user = await pool.query(loginQuery, [email]);
    // Utilisateur récupéré
    const userFetched: User = user.rows[0];
    const hashedPassword: string = userFetched.password;

    // Comparaison mot de passe
    if (!password || !(await bcrypt.compare(password, hashedPassword))) {
      return res
        .status(UNAUTHORIZED)
        .send("Le mot de passe entré est incorrect");
    }

    if (!userFetched) {
      return res
        .status(UNAUTHORIZED)
        .json({ error: "L'utilisateur n'existe pas" });
    }

    // Mise à jour date de connexion
    await pool.query(updateDateQuery, [loginDate, email]);

    const token = jwt.sign({ user: userFetched }, SECRET_KEY, {
      expiresIn: "1h",
    });

    // Réponse
    const loginResponse: LoginResponse = {
      message: "L'utilisateur est connecté",
      token: token,
      User: userFetched,
    };

    res.status(200).json({ loginResponse });
  } catch (error: unknown) {
    res
      .status(500)
      .json({ error: "La connexion a échouée, une erreur est survenue" });
  }
};

// Route protégée
export const ProtectedController = async (req: Request, res: Response) => {
  res.status(OK).json(`Accès à la route protégé !`);
};
