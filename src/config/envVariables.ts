import dotenv from "dotenv";
dotenv.config();

// Général
export const SERVER_PORT = process.env.SERVER_PORT;

// Base de données
export const DATABASE_HOST = process.env.DB_HOST;
export const DATABASE_USER = process.env.DB_USER;
export const DATABASE_PORT: number = 5433;
export const DATABASE_PASSWORD = process.env.DB_PASSWORD;
export const DATABASE_NAME = process.env.DB_NAME;

// JWT
export const SECRET_KEY = process.env.JWT_SECRET;
