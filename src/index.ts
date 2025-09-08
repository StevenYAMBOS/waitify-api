import bodyParser from "body-parser";
import express, { Response } from "express";
import { neon } from "@neondatabase/serverless";
import "dotenv/config";

const app = express();
const port = process.env.PORT;

app.use(bodyParser.urlencoded({ extended: true }));

app.get("/", async (_, res: Response) => {
  const sql = neon(`${process.env.DATABASE_URL}`);
  const response = await sql`SELECT version()`;
  const { version } = response[0];
  res.json({ version });
});

app.listen(port, () => {
  console.log(`API lançée à l'adresse : http://localhost:${port}`);
});
