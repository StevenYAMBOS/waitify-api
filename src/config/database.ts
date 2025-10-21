import pg from "pg";
import dotenv from "dotenv";
dotenv.config();

const { Pool } = pg;

const DATABASE_HOST = process.env.DB_HOST;
const DATABASE_USER = process.env.DB_USER;
const DATABASE_PORT: number = 5433;
const DATABASE_PASSWORD = process.env.DB_PASSWORD;
const DATABASE_NAME = process.env.DB_NAME;

export const pool = new Pool({
  host: DATABASE_HOST,
  user: DATABASE_USER,
  port: DATABASE_PORT,
  password: DATABASE_PASSWORD,
  database: DATABASE_NAME,
});
