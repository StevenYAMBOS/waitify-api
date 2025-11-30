import { v4 as uuidv4 } from "uuid";
import bcrypt from "bcryptjs";
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
  ValidationError,
} from "../models/authModels";
import { validateEmail, validatePassword } from "../validators/authValidators";
import {
  ERROR_MESSAGES,
  USER_MESSAGES,
  VALIDATION,
} from "../../config/constants";
import { UserRepository } from "../../users/repositories/userRepositories";

const userRepository = new UserRepository();

export const RegisterUserService = async (
  input: RegisterRequest
): Promise<{ user?: RegisterResponse; errors?: ValidationError[] }> => {
  const { email, password } = input;

  const errors: ValidationError[] = [];

  const emailError = validateEmail(email);
  if (emailError) errors.push(emailError);

  const passwordError = validatePassword(password);
  if (passwordError) errors.push(passwordError);

  if (errors.length > 0) {
    return { errors };
  }

  const emailExists = await userRepository.emailExists(email);

  if (emailExists) {
    return {
      errors: [
        {
          champs: USER_MESSAGES.EMAIL,
          message: USER_MESSAGES.EMAIL_ALREADY_EXISTS,
        },
      ],
    };
  }

  const uuid = uuidv4();
  const date = new Date();
  const hashedPassword = await bcrypt.hash(password, 10);

  const userCreated = await userRepository.create({
    id: uuid,
    email: email,
    password: hashedPassword,
    created_at: date,
    updated_at: date,
  });

  const user: RegisterResponse = {
    message: USER_MESSAGES.CREATION_SUCCESS,
    User: {
      id: userCreated.uuid,
      email: userCreated.email,
      createdAt: userCreated.date,
    },
  };

  return { user };
};

export const LoginUserService = async (
  input: LoginRequest
): Promise<{ user?: LoginResponse; error?: string }> => {
  const { email, password } = input;

  const emailError = validateEmail(email);
  const passwordError = validatePassword(password);

  if (emailError || passwordError) {
    return { error: ERROR_MESSAGES.INVALID_EMAIL_OR_PASSWORD };
  }

  const userFetched = await userRepository.findByEmail(email);

  if (!userFetched) {
    return { error: USER_MESSAGES.DOES_NOT_EXISTS };
  }

  const hashedPassword: string = userFetched.password;

  const isPasswordValid = await bcrypt.compare(password, hashedPassword);

  if (!isPasswordValid) {
    return { error: VALIDATION.INVALID_PASSWORD };
  }

  const loginDate = new Date();

  await userRepository.updateLastLogin(email, loginDate);

  const { password: _pwd, ...userWithoutPassword } = userFetched;

  return { user: userWithoutPassword as LoginResponse };
};
