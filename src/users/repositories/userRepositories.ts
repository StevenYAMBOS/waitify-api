import { CreateUserData } from "../../auth/models/authModels";
import { pool } from "../../config/database";
import { User } from "../models/userModels";

export class UserRepository {
  async findByEmail(email: string): Promise<User | null> {
    const query = "SELECT * FROM users WHERE email = $1";
    const result = await pool.query(query, [email]);
    return result.rows[0] || null;
  }

  async emailExists(email: string): Promise<boolean> {
    const query = "SELECT EXISTS(SELECT 1 FROM users WHERE email = $1)";
    const result = await pool.query(query, [email]);
    return result.rows[0].exists;
  }

  async create(userData: CreateUserData): Promise<User> {
    const query = `
      INSERT INTO users (id, email, password, created_at, updated_at)
      VALUES ($1, $2, $3, $4, $5)
      RETURNING *
    `;
    const result = await pool.query(query, [
      userData.id,
      userData.email,
      userData.password,
      userData.created_at,
      userData.updated_at,
    ]);
    return result.rows[0];
  }

  async updateLastLogin(email: string, loginDate: Date): Promise<void> {
    const query = "UPDATE users SET last_login = $1 WHERE email = $2";
    await pool.query(query, [loginDate, email]);
  }
}
