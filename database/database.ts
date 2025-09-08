import { Pool } from "pg";
import "dotenv/config";

const { DATABASE_HOST, DATABASE_DB, DATABASE_USER, DATABASE_PASSWORD } =
  process.env;

export const pool = new Pool({
  host: DATABASE_HOST,
  database: DATABASE_DB,
  user: DATABASE_USER,
  password: DATABASE_PASSWORD,
  port: 5432,
});
