import bodyParser from "body-parser";
import express, { Request, Response } from "express";

const app = express();
const port = process.env.PORT;

app.use(bodyParser.urlencoded({ extended: true }));

app.get("/", (req: Request, res: Response) => {
  res.send("Hello world");
});

app.listen(port, () => {
  console.log(`API lançée à l'adresse : http://localhost:${port}`);
});
