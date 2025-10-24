import { Request, Response } from "express";
import {
  AddBusinessResponse,
  Business,
  BusinessEntry,
} from "../models/businessModels";
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
import QRCode from "qrcode";

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

//  Créer une entreprise + générer le QR Code
export const AddBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const { name, businessType, phoneNumber, address, city, zipCode, country } =
      req?.body;

    // Générer l'id de l'entreprise + le token du QR Code
    const uuid: string = uuidv4();
    // Date du jour
    const now: Date = new Date();
    // Id de l'utilisateur connecté
    const UserId: string = req?.user?.id;

    // Query
    const query: string = `INSERT INTO businesses (name, UserId, business_type, phone_number, address, city, zip_code, country, qr_code_token, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) RETURNING *`;
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
      uuid,
      now,
      now,
    ];

    // Insérer les informations de l'entreprise en base de données
    const business = await pool.query(query, values);

    // Information de l'entreprise créé
    const businessCreated: Business = business.rows[0];

    // QRCode
    const url = "https://stevenyambos.fr";
    const qrCodeImage = await QRCode.toDataURL(url);
    const qrCodeData: string = `<img src="${qrCodeImage}" alt="QR Code"/>`;

    // Réponse entreprise créé
    const response: AddBusinessResponse = {
      Business: businessCreated,
      QRCode: qrCodeData,
    };

    res.status(OK).send(response);
  } catch (error: unknown) {
    res.status(INTERNAL_SERVER_ERROR).json({
      message: BUSINESS_NOT_FOUND,
      error: error,
    });
  }
};
