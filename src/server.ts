import express, { Request, Response } from "express";
import bodyParser from "body-parser";
import dotenv from "dotenv";
import authRouter from "./auth/routes/authRouter";
dotenv.config();

const port = process.env.SERVER_PORT;
const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

app.get("/health", (req: Request, res: Response) => {
  res.json("Tout va bien !");
  res.status(200);
});

app.use("/auth", authRouter);

app.listen(port, () => {
  console.log(
    `L'application est lançée à l'adresse : http://localhost:${port}`
  );
});
