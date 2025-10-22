import { JwtPayload } from "jsonwebtoken";
import { User } from "../../users/models/userModels";

type RegisterRequest = {
  username: string;
  email: string;
  password: string;
};

type RegisterResponse = {
  message: string;
  user: User;
};

type LoginRequest = {
  email: string;
  password: string;
};

type LoginEntry = {
  id?: string;
  email: string;
  password: string;
  profile_picture: string;
  auth_provider: string;
  createdAt: string;
  updatedAt: string;
  lastLogin: string;
};

interface LoginResponse {
  message: string;
  token: string;
  User: LoginEntry;
}

interface CustomRequest extends Request {
  token: string | JwtPayload;
}

export {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
  CustomRequest,
};
