import { pool } from "../../config/database";
import { v4 as uuidv4 } from "uuid";
import bcrypt from "bcryptjs";
import { ASSETS } from "../../config/constants";
import {
  RegisterUserInput,
  RegisterUserOutput,
  ValidationError,
} from "../models/authModels";
import { validateEmail, validatePassword } from "../validators/authValidators";

export const RegisterUserService = async (
  input: RegisterUserInput
): Promise<{ user?: RegisterUserOutput; errors?: ValidationError[] }> => {
  const { email, password, profile_picture } = input;

  const errors: ValidationError[] = [];

  const emailError = validateEmail(email);
  if (emailError) errors.push(emailError);

  const passwordError = validatePassword(password);
  if (passwordError) errors.push(passwordError);

  if (errors.length > 0) {
    return { errors };
  }

  const existingUserQuery = `SELECT id FROM users WHERE email = $1`;
  const existingUser = await pool.query(existingUserQuery, [email]);

  if (existingUser.rows.length > 0) {
    return {
      errors: [
        {
          champs: "Email",
          message: "Un utilisateur avec cet email existe déjà",
        },
      ],
    };
  }

  // Générer l'id
  const uuid = uuidv4();

  // Date du jour
  const date = new Date();

  // Hash password
  const hashedPassword = await bcrypt.hash(password, 10);

  // Insertion en base de données
  const query = `INSERT INTO users (id, email, password, profile_picture, created_at, updated_at) 
                 VALUES ($1, $2, $3, $4, $5, $6)`;

  await pool.query(query, [
    uuid,
    email,
    hashedPassword,
    profile_picture || ASSETS.PLACEHOLDER_IMAGE,
    date,
    date,
  ]);

  const user: RegisterUserOutput = {
    id: uuid,
    email: email,
    profile_picture: profile_picture || ASSETS.PLACEHOLDER_IMAGE,
    createdAt: date,
  };

  return { user };
};

// Service de connexion
interface LoginUserInput {
  email: string;
  password: string;
}

interface LoginUserOutput {
  id: string;
  email: string;
  profile_picture: string;
  created_at: Date;
  updated_at: Date;
  last_login: Date | null;
}

export const LoginUserService = async (
  input: LoginUserInput
): Promise<{ user?: LoginUserOutput; error?: string }> => {
  const { email, password } = input;

  // Validation des données
  const emailError = validateEmail(email);
  const passwordError = validatePassword(password);

  if (emailError || passwordError) {
    return { error: "Email ou mot de passe invalide" };
  }

  // Date de connexion
  const loginDate = new Date();

  // Query connexion
  const loginQuery = `SELECT * FROM users WHERE email=$1`;

  // Requête de connexion
  const result = await pool.query(loginQuery, [email]);

  if (result.rows.length === 0) {
    return { error: "L'utilisateur n'existe pas" };
  }

  const userFetched = result.rows[0];
  const hashedPassword: string = userFetched.password;

  // Comparaison mot de passe
  const isPasswordValid = await bcrypt.compare(password, hashedPassword);

  if (!isPasswordValid) {
    return { error: "Le mot de passe entré est incorrect" };
  }

  // Mise à jour date de connexion
  const updateDateQuery = `UPDATE users SET last_login = $1 WHERE email=$2`;
  await pool.query(updateDateQuery, [loginDate, email]);

  // Retourner l'utilisateur (sans le mot de passe)
  const { password: _, ...userWithoutPassword } = userFetched;

  return { user: userWithoutPassword };
};
