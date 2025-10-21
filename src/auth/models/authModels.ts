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
  username: string;
  email: string;
  password: string;
};

export { RegisterRequest, RegisterResponse, LoginRequest };
