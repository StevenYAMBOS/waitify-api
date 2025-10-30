import request from "supertest";
import app from "../server";

describe("GET /auth/protected", () => {
  it("Devrait retourner un code 200", async () => {
    const res = await request(app).get("/auth/protected");
    expect(res.statusCode).toBe(200);
  });
});
