import express from "express";
import dotenv from "dotenv";

dotenv.config();

const app = express();
const port = process.env.SERVER_PORT;

app.get("/", (req, res) => {
  res.send("Hello World!");
});

app.listen(port, () => {
  return console.log(
    `L'application est lançé à l'adresse : http://localhost:${port}`
  );
});
