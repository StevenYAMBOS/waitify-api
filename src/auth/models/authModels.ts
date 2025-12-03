import { JwtPayload } from "jsonwebtoken";

export interface RegisterRequest {
  email: string;
  password: string;
  profilePicture?: string;
}

export interface CreateUserData {
  id: string;
  email: string;
  password: string;
  profile_picture?: string;
  google_id?: string;
  created_at: Date;
  updated_at: Date;
}

export interface RegisterResponse {
  message: string;
  User: {
    id: string;
    email: string;
    createdAt: Date;
    updatedAt: Date;
  };
}

export interface ValidationError {
  champs: string;
  message: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  message: string;
  token: string;
  User: {
    id: string;
    email: string;
    profile_picture: string;
    created_at: Date;
    updated_at: Date;
    last_login: Date | null;
  };
}

export interface GoogleTokenResponse {
  access_token: string;
  expires_in: number;
  refresh_token?: string;
  scope: string;
  token_type: string;
  id_token: string;
}

export interface GoogleUserInfo {
  sub: string; // Google User ID
  email: string;
  email_verified: boolean;
  name: string;
  given_name: string;
  family_name: string;
  picture: string;
  locale: string;
}

export interface CustomRequest extends Request {
  token: string | JwtPayload;
}
