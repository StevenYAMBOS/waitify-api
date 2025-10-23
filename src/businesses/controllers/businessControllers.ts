import { Request, Response } from "express";
import { Business, BusinessEntry } from "../models/businessModels";
import { pool } from "../../config/database";
import {
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  BUSINESS_NOT_FOUND,
  GET_METHOD,
  INTERNAL_SERVER_ERROR,
  OK,
  POST_METHOD,
} from "../../config/constants";
import { v4 as uuidv4 } from "uuid";

// Récupérer les informations d'une entreprise
export const GetBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== GET_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params?.id;
    // Query
    const query: string = `SELECT * FROM businesses WHERE id = $1`;
    // Récupérer les informations de l'entreprise
    const response = await pool.query(query, [idParam]);

    // Entreprise récupérée
    const businessFetched: Business = response?.rows[0];
    res.status(OK).send(businessFetched);
  } catch (error: unknown) {
    res.status(INTERNAL_SERVER_ERROR).json({
      message: BUSINESS_NOT_FOUND,
      error: error,
    });
  }
};

// Récupérer les informations d'une entreprise
export const AddBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const {
      name,
      UserId,
      businessType,
      phoneNumber,
      address,
      city,
      zipCode,
      country,
    } = req?.body;

    // Générer l'id
    const uuid: string = uuidv4();
    // Date du jour
    const now: Date = new Date();
    /* Id de l'utilisateur connecté
    const userConnectedId: string = req?.user?.id;
    */
    // Query
    const query: string = `INSERT INTO businesses (name, UserId, business_type, phone_number, address, city, zip_code, country, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) RETURNING *`;
    // Valeurs
    const values: string[] = [
      uuid,
      name,
      UserId,
      businessType,
      phoneNumber,
      address,
      city,
      zipCode,
      country,
      now,
      now,
    ];

    // Insérer les informations de l'entreprise en base de données
    const response = await pool.query(query, values);

    // Entreprise créé
    const business: BusinessEntry = {
      id: uuid,
      UserId: UserId,
      name: name,
      businessType: businessType,
      phoneNumber: phoneNumber,
      address: address,
      city: city,
      zipCode: zipCode,
      country: country,
      createdAt: now,
      updatedAt: now,
    };

    console.log("RÉPONSE : ", response.rows[0]);
    console.log("ENTREPRISE : ", business);

    res.status(OK).send(business);
  } catch (error: unknown) {
    res.status(INTERNAL_SERVER_ERROR).json({
      message: BUSINESS_NOT_FOUND,
      error: error,
    });
  }
};
