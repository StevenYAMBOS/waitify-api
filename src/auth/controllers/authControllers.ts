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
    "https://s1.qwant.com/thumbr/474x474/2/8/592dfd498dc2ad4f9171c77dbd60d95525c81b69e1773b6a1e1965cfd3b03d/OIP.kP4L729KY5ve4Tj54TvGcAHaHa.jpg?u=https%3A%2F%2Fthvnext.bing.com%2Fth%2Fid%2FOIP.kP4L729KY5ve4Tj54TvGcAHaHa%3Fcb%3D12%26pid%3DApi%26ucfimg%3D1&q=0&b=1&p=0&a=0";

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

  const response = await pool.query(query, [
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

  console.log("UTILISATEUR : ", response);

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
