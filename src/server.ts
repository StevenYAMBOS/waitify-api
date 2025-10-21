import express from "express";
import bodyParser from "body-parser";
import dotenv from "dotenv";
dotenv.config();

const port = process.env.SERVER_PORT;
const app = express();
app.use(bodyParser.urlencoded({ extended: true }));

app.listen(port, () => {
  console.log(`L'application est lançé à l'adresse : http://localhost:${port}`);
});
