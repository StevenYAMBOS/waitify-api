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

interface LoginResponse {
  message: string;
  token: string;
  User: User;
}

export { RegisterRequest, RegisterResponse, LoginRequest, LoginResponse };
