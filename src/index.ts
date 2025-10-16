import dotenv from "dotenv";
import express from "express";

dotenv.config();

const port = process.env.SERVER_PORT || 3000;
const app = express();

app.listen(port, () => {
  console.log(`L'application est lançé à l'adresse : http://localhost:${port}`);
});
