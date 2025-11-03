import express from "express";
import bodyParser from "body-parser";
import authRouter from "./auth/routes/authRouter";
import userRouter from "./users/routes/userRouter";
import businessRouter from "./businesses/routes/businessRouter";
import { SERVER_PORT } from "./config/envVariables";
import {
  AUTH_PATH,
  BUSINESS_PATH,
  QUEUES_PATH,
  USER_PATH,
} from "./config/constants";
import queueRouter from "./queues/routes/queueRouter";

const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Routes d'authentification
app.use(AUTH_PATH, authRouter);
// Routes utilisateurs
app.use(USER_PATH, userRouter);
// Routes entreprises
app.use(BUSINESS_PATH, businessRouter);
// Routes files d'attentes
app.use(QUEUES_PATH, queueRouter);

app.listen(SERVER_PORT, () => {
  console.log(
    `L'application est lançée à l'adresse : http://localhost:${SERVER_PORT}`,
  );
});

export default app;
