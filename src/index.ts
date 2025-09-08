import bodyParser from "body-parser";
import express, { Request, Response } from "express";
// import cors from "cors";
import "dotenv/config";
import { pool } from "../database/database";

const app = express();
const PORT = process.env.PORT;

// app.use(cors);
app.use(bodyParser.urlencoded({ extended: true }));

app.get("/", (req: Request, res: Response) => {
  res.send("Hello world");
});

app.listen(PORT, () => {
  console.log(`API lançée à l'adresse : http://localhost:${PORT}`);
  console.log(`Connexion à la base de données : `, pool);
});
