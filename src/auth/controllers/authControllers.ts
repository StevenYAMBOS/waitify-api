import { Request, Response } from "express";
import { pool } from "../../config/database.js";
import { v4 as uuidv4 } from "uuid";
import jwt from "jsonwebtoken";
import bcrypt from "bcryptjs";
import { LoginResponse } from "../models/authModels.js";
import { User } from "../../users/models/userModels.js";
import { SECRET_KEY } from "../../config/variables.js";

// Inscription
export const RegisterController = async (req: Request, res: Response) => {
  if (req.method !== "POST") {
    res.status(400).send("Mauvaise méthode HTTP.");
  }

  try {
    const { email, password, profile_picture } = req.body;
    // Générer l'id
    const uuid = uuidv4();
    // Date du jour
    const date = new Date();
    // Hash password
    const hashedPassword = await bcrypt.hash(password, 10);
    const hardCodedProfilePicture: string =
      "https://media.istockphoto.com/id/985915172/fr/vectoriel/%C3%A9checs-de-checker-vecteur-abstrait-sans-soudure.jpg?s=612x612&w=0&k=20&c=4BLWcNYZe9uykbirGZHc2_0zZC0pIIKS4Tvt19oj8TQ=";

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
      profile_picture || hardCodedProfilePicture,
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

    console.log("UTILISATEUR : ", user);

    res.status(201).json({ message, user });
  } catch (error: unknown) {
    res.status(500).json({ error: "L'inscription a échouée." });
  }
};

// Connexion
export const LoginController = async (req: Request, res: Response) => {
  if (req.method !== "POST") {
    res.status(400).send("Mauvaise méthode HTTP.");
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
      return res.status(401).json({
        status: "Mauvaise requête",
        message: "Erreur lors de la connexion",
        statusCode: 401,
      });
    }

    // Requête de connexion
    const user = await pool.query(loginQuery, [email]);
    // Utilisateur récupéré
    const userFetched: User = user.rows[0];
    const userId: string = userFetched.id;
    const hashedPassword: string = userFetched.password;

    // Comparaison mot de passe
    if (!password || !(await bcrypt.compare(password, hashedPassword))) {
      return res.status(401).send("Le mot de passe entré est incorrect");
    }

    if (!userFetched) {
      return res.status(401).json({ error: "L'utilisateur n'existe pas" });
    }

    // Mise à jour date de connexion
    await pool.query(updateDateQuery, [loginDate, email]);

    const token = jwt.sign({ id: userId }, SECRET_KEY, {
      expiresIn: "1h",
    });

    // Réponse
    const loginResponse: LoginResponse = {
      message: "L'utilisateur est connecté",
      token: token,
      User: userFetched,
    };

    console.log(loginResponse);

    res.status(200).json({ loginResponse });
  } catch (error: unknown) {
    res
      .status(500)
      .json({ error: "La connexion a échouée, une erreur est survenue" });
  }
};

// Route protégée
export const ProtectedController = async (req: Request, res: Response) => {
  res.status(200).json(`Accès à la route protégé !`);
};

/* 
export const getUsers = async (
  req: Request,
  res: Response
): Promise<Response> => {
  try {
    const response: QueryResult = await pool.query(
      "SELECT * FROM users ORDER BY id ASC"
    );
    return res.status(200).json(response.rows);
  } catch (e) {
    console.log(e);
    return res.status(500).json("Internal Server error");
  }
};

export const getUserById = async (
  req: Request,
  res: Response
): Promise<Response> => {
  const id = parseInt(req.params.id);
  const response: QueryResult = await pool.query(
    "SELECT * FROM users WHERE id = $1",
    [id]
  );
  return res.json(response.rows);
};

export const updateUser = async (req: Request, res: Response) => {
  const id = parseInt(req.params.id);
  const { name, email } = req.body;

  const response = await pool.query(
    "UPDATE users SET name = $1, email = $2 WHERE id = $3",
    [name, email, id]
  );
  console.log(response);

  res.json("User Updated Successfully");
};

export const deleteUser = async (req: Request, res: Response) => {
  const id = parseInt(req.params.id);
  await pool.query("DELETE FROM users where id = $1", [id]);
  res.json(`User ${id} deleted Successfully`);
};
*/
