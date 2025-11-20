import request from "supertest";
import app from "../server";
import { LoginRequest } from "../auth/models/authModels";
import {
  HTTP_METHODS,
  HTTP_STATUS,
  ROUTES_AUTH,
  TEST_DATA,
} from "../config/constants";

// Requête test pour connecter l'utilisateur
const loginRequest: LoginRequest = {
  email: TEST_DATA.TEST_USER_EMAIL,
  password: TEST_DATA.TEST_USER_PASSWORD,
};

describe(`${HTTP_METHODS.GET} ${ROUTES_AUTH.BASE}${ROUTES_AUTH.LOGIN}`, () => {
  it(`${TEST_DATA.TEST_LOGIN_SHOULD_SUCCEED}`, async () => {
    return request(app)
      .post(`${ROUTES_AUTH.BASE}${ROUTES_AUTH.LOGIN}`)
      .send(loginRequest)
      .expect(HTTP_STATUS.OK);
  });
});
