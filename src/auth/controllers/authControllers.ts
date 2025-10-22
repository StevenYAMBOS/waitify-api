import { Request, Response } from "express";
import { pool } from "../../config/database.js";
import { v4 as uuidv4 } from "uuid";
import jwt from "jsonwebtoken";
import bcrypt from "bcryptjs";

// Inscription
export const Register = async (req: Request, res: Response) => {
  if (req.method !== "POST") {
    res.status(400).send("Mauvaise méthode HTTP.");
  }

  const { email, password, profile_picture } = req.body;
  // Générer l'id
  const uuid = uuidv4();
  // Date du jour
  const date = new Date();
  // Hash password
  const hashedPassword = await bcrypt.hash(password, 10);
  const hardCodedProfilePicture: string =
    "https://media.istockphoto.com/id/985915172/fr/vectoriel/%C3%A9checs-de-checker-vecteur-abstrait-sans-soudure.jpg?s=612x612&w=0&k=20&c=4BLWcNYZe9uykbirGZHc2_0zZC0pIIKS4Tvt19oj8TQ=";

  // Insetion en base de données
  const query: string = `INSERT INTO users (id, email, password, profile_picture, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6)`;

  /* ------ Restrictions ------ */

  let errors = [];

  if (
    !email ||
    email.length == 0 ||
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
  ) {
    errors.push({
      champs: "Email",
      message: "L'adresse email est invalide",
    });
  }

  if (!password || password.length == 0) {
    errors.push({
      champs: "Mot de passe",
      message: "Le mot de passe est invalide",
    });
  }

  if (errors.length > 0) {
    return res.status(422).json({
      errors: errors,
    });
  }

  await pool.query(query, [
    uuid,
    email,
    hashedPassword,
    profile_picture || hardCodedProfilePicture,
    date,
    date,
  ]);

  const user = {
    id: uuid,
    email: email,
    profile_picture: profile_picture,
    createdAt: date,
  };

  const message: string = "Utilisateur créé avec succès";

  console.log("UTILISATEUR : ", user);

  res.status(201).json({ message, user });
};

/* 
// Connexion
export const Login = async (req: Request, res: Response) => {
  if (req.method !== "POST") {
    res.status(400).send("Mauvaise méthode HTTP.");
  }

  const { email, password } = req.body;
  // Date du jour
  const date = new Date();
  // Query
  const query: string = `SELECT * FROM users WHERE id=$1`;

  if (
    !email ||
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) ||
    !password ||
    password.length == 0
  ) {
    return res.status(401).json({
      status: "Mauvaise requête",
      message: "Erreur lors de la connexion",
      statusCode: 401,
    });
  }

  if (!password || !(await bcrypt.compare(password, req.body.password))) {
    return res.status(401).send("Mauvais identifiants de connexion");
  }

  const user = await pool.query(query, [email]);

  /*
if (user.rowCount > 0) {
    let { email, password } = user.rows[0];
    return res.status(200).json({
      status: "Succès",
      message: "Connexion réussie",
      data: {
        accessToken: await jwt.generateToken({
          userId: userid,
        }),
        user: {
          userId: "" + userid,
          firstName: firstname,
          lastName: lastname,
          email: email,
          phone: phone,
        },
      },
    });
  } else {
    return res.status(401).json({
      status: "Bad request",
      message: "Authentication failed",
      statusCode: 401,
    });
  }
    

  const message: string = "Connexion réussie !";

  res.status(200).send(message);
};
 */
/* 
export const getUsers = async (
  req: Request,
  res: Response
): Promise<Response> => {
  try {
    const response: QueryResult = await pool.query(
      "SELECT * FROM users ORDER BY id ASC"
    );
    return res.status(200).json(response.rows);
  } catch (e) {
    console.log(e);
    return res.status(500).json("Internal Server error");
  }
};

export const getUserById = async (
  req: Request,
  res: Response
): Promise<Response> => {
  const id = parseInt(req.params.id);
  const response: QueryResult = await pool.query(
    "SELECT * FROM users WHERE id = $1",
    [id]
  );
  return res.json(response.rows);
};

export const updateUser = async (req: Request, res: Response) => {
  const id = parseInt(req.params.id);
  const { name, email } = req.body;

  const response = await pool.query(
    "UPDATE users SET name = $1, email = $2 WHERE id = $3",
    [name, email, id]
  );
  console.log(response);

  res.json("User Updated Successfully");
};

export const deleteUser = async (req: Request, res: Response) => {
  const id = parseInt(req.params.id);
  await pool.query("DELETE FROM users where id = $1", [id]);
  res.json(`User ${id} deleted Successfully`);
};
*/
