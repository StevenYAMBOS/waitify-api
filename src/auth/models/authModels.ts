import { JwtPayload } from "jsonwebtoken";
import { User } from "../../users/models/userModels";

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  message: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  message: string;
  token: string;
  User: User;
}

export interface CustomRequest extends Request {
  token: string | JwtPayload;
}
