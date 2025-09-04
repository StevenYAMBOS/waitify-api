import express, { Request, Response } from 'express';

const app = express();
const port: number = 3000;

app.get('/', (req: Request, res: Response) => {
  res.send('Hello world');
});

app.listen(port, () => {
  console.log(`API lançée à l'adresse : http://localhost:${port}`);
});