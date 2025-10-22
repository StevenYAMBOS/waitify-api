import express, { Request, Response } from "express";
import bodyParser from "body-parser";
import authRouter from "./auth/routes/authRouter";
import { SERVER_PORT } from "./config/variables";

const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

app.get("/health", (req: Request, res: Response) => {
  res.json("Tout va bien !");
  res.status(200);
});

app.use("/auth", authRouter);

app.listen(SERVER_PORT, () => {
  console.log(
    `L'application est lançée à l'adresse : http://localhost:${SERVER_PORT}`
  );
});
