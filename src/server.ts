import express from "express";
import bodyParser from "body-parser";
import authRouter from "./auth/routes/authRouter";
import userRouter from "./users/routes/userRouter";
import businessRouter from "./businesses/routes/businessRouter";
import { SERVER_PORT } from "./config/envVariables";
import {
  ROUTES_AUTH,
  ROUTES_BUSINESSES,
  ROUTES_QUEUES,
  ROUTES_USERS,
} from "./config/constants";
import queueRouter from "./queues/routes/queueRouter";

const app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

app.use(ROUTES_AUTH.BASE, authRouter);
app.use(ROUTES_USERS.BASE, userRouter);
app.use(ROUTES_BUSINESSES.BASE, businessRouter);
app.use(ROUTES_QUEUES.BASE, queueRouter);

app.listen(SERVER_PORT, () => {
  console.log(
    `L'application est lançée à l'adresse : http://localhost:${SERVER_PORT}`
  );
});

export default app;
