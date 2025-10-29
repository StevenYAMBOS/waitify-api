import request from "supertest";
import app from "../server";

describe("GET /auth/protected", () => {
  it("Devrait retourner un code 200", async () => {
    const res = await request(app).get("/auth/protected"); // simulate GET request
    expect(res.statusCode).toBe(200); // assert status code is 200
  });
});
