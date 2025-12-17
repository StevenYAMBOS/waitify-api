import request from "supertest";
import app from "../server";
import jwt from "jsonwebtoken";
import { SECRET_KEY } from "../config/envVariables";
import {
  AUTH,
  ERROR_MESSAGES,
  HTTP_METHODS,
  HTTP_STATUS,
  ROUTES_USERS,
  TEST_DATA,
} from "../config/constants";
import { User } from "../users/models/userModels";

const mockUser: User = {
  id: "123e4567-e89b-12d3-a456-426614174000",
  email: TEST_DATA.TEST_USER_EMAIL,
  password: "",
  firstName: "Test",
  lastName: "Test",
  phoneNumber: "+33612345678",
  companyName: "Entreprise test",
  isActive: true,
  subscriptionStatus: "active",
  trialEndsAt: new Date(),
  subscriptionPlanId: "plan-123",
  googleId: "",
  profilePicture: "https://example.com/avatar.jpg",
  authProvider: "email",
  createdAt: new Date(),
  updatedAt: new Date(),
  lastLogin: new Date(),
};

const generateValidToken = (user: User): string => {
  return jwt.sign({ user }, SECRET_KEY, { expiresIn: "1h" });
};

describe(`${HTTP_METHODS.GET} ${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`, () => {
  it("devrait retourner le profil utilisateur avec un token valide", async () => {
    const validToken = generateValidToken(mockUser);

    const response = await request(app)
      .get(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .set(AUTH.HEADER_NAME, `${AUTH.BEARER_PREFIX}${validToken}`)
      .expect(HTTP_STATUS.OK);
    expect(response.body).toHaveProperty("id");
    expect(response.body).toHaveProperty("email");
    expect(response.body.email).toBe(mockUser.email);
  });

  it("devrait retourner 401 (UNAUTHORIZED) sans token d'authentification", async () => {
    const response = await request(app)
      .get(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .expect(HTTP_STATUS.UNAUTHORIZED);

    expect(response.text).toBe(AUTH.UNAUTHORIZED_ACCESS);
  });

  it("devrait retourner 403 (FORBIDDEN) avec un token invalide", async () => {
    const invalidToken = jwt.sign({ user: mockUser }, "wrong-secret-key", {
      expiresIn: "1h",
    });

    const response = await request(app)
      .get(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .set(AUTH.HEADER_NAME, `${AUTH.BEARER_PREFIX}${invalidToken}`)
      .expect(HTTP_STATUS.FORBIDDEN);

    expect(response.text).toBe(AUTH.INVALID_TOKEN);
  });

  it("devrait retourner 403 (FORBIDDEN) avec un token expiré", async () => {
    const expiredPayload = {
      user: mockUser,
      exp: Math.floor(Date.now() / 1000) - 3600,
    };
    const expiredToken = jwt.sign(expiredPayload, SECRET_KEY);

    const response = await request(app)
      .get(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .set(AUTH.HEADER_NAME, `${AUTH.BEARER_PREFIX}${expiredToken}`)
      .expect(HTTP_STATUS.FORBIDDEN);

    expect(response.text).toBe(AUTH.INVALID_TOKEN);
  });
  it("devrait retourner 400 (BAD_REQUEST) avec une méthode HTTP incorrecte", async () => {
    const validToken = generateValidToken(mockUser);

    const response = await request(app)
      .post(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .set(AUTH.HEADER_NAME, `${AUTH.BEARER_PREFIX}${validToken}`)
      .expect(HTTP_STATUS.BAD_REQUEST);

    expect(response.text).toBe(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  });

  it("devrait retourner une erreur avec un token sans préfixe 'Bearer '", async () => {
    const validToken = generateValidToken(mockUser);

    const response = await request(app)
      .get(`${ROUTES_USERS.BASE}${ROUTES_USERS.PROFILE}`)
      .set(AUTH.HEADER_NAME, validToken)
      .expect(HTTP_STATUS.UNAUTHORIZED);

    expect(response.text).toBe(AUTH.UNAUTHORIZED_ACCESS);
  });
});
