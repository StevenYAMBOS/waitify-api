import request from "supertest";
import app from "../server";
import { LoginRequest } from "../auth/models/authModels";
import {
  AUTH_PATH,
  GET_METHOD,
  LOGIN_PATH,
  OK,
  TEST_LOGIN_EMAIL,
  TEST_LOGIN_PASSWORD,
  TEST_LOGIN_RULE_MESSAGE,
} from "../config/constants";

// Requête test pour connecter l'utilisateur
const loginRequest: LoginRequest = {
  email: TEST_LOGIN_EMAIL,
  password: TEST_LOGIN_PASSWORD,
};

describe(`${GET_METHOD + AUTH_PATH + LOGIN_PATH}`, () => {
  it(`${TEST_LOGIN_RULE_MESSAGE}`, async () => {
    return request(app)
      .post(`${AUTH_PATH + LOGIN_PATH}`)
      .send(loginRequest)
      .expect(OK);
  });
});
