import { Request, Response } from "express";
import { Business } from "../models/businessModels";
import { pool } from "../../config/database";
import {
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  GET_METHOD,
  INTERNAL_SERVER_ERROR,
  INTERNAL_SERVER_ERROR_MESSAGE,
  OK,
  POST_METHOD,
  QRCODE_TOKEN_PATH,
  WAITIFY_URL,
} from "../../config/constants";
import { v4 as uuidv4 } from "uuid";
import QRCode from "qrcode";
import { PassThrough } from "stream";

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
      message: INTERNAL_SERVER_ERROR_MESSAGE,
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
    // Générer l'id de l'entreprise + le token du QR Code
    const uuid: string = uuidv4();
    console.log("Initialisation de l'Id", uuid);

    // Générer l'id de l'entreprise + le token du QR Code
    const qrCodeToken: string = uuidv4();
    console.log("Initialisation du token", qrCodeToken);

    // Date du jour
    const now: Date = new Date();

    /*
    // Id de l'utilisateur connecté
    const user: User = req.user;
    const UserId: string = user.id;
    console.log("ID USER : ", UserId);
    */

    // Query
    const query: string = `INSERT INTO businesses (id, name, UserId, business_type, phone_number, address, city, zip_code, country, qr_code_token, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12) RETURNING *`;
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
      qrCodeToken,
      now,
      now,
    ];

    // Insérer les informations de l'entreprise en base de données
    await pool.query(query, values);

    /*
    // Information de l'entreprise créé
    const business = await pool.query(query, values);
    const businessCreated: Business = business.rows[0];
    */

    // Contenu du du QRCode
    const content: string = WAITIFY_URL + QRCODE_TOKEN_PATH + `${qrCodeToken}`;
    // Taille de l'image du QRCode
    const size: number = 256;
    // Type d'image du QRCode
    const imgExt = "png";
    const errType = "H";
    // Envoie des données (bytes) vers la réponse
    const qrStream = new PassThrough();

    // Génération du QRCode
    await QRCode.toFileStream(qrStream, content, {
      type: imgExt,
      width: size,
      errorCorrectionLevel: errType,
    });

    // Renvoi l'image du QRCode
    qrStream.pipe(res);
  } catch (error: unknown) {
    console.error(INTERNAL_SERVER_ERROR_MESSAGE, error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }
};

// Test
export const TestController = async (req: Request, res: Response) => {
  try {
    const content: string = "https://stevenyambos.fr";
    const size: number = 256;
    const imgExt = "png";
    const errType = "H";
    // Envoie des données (bytes) vers la réponse
    const qrStream = new PassThrough();

    // Génération du QRCode
    await QRCode.toFileStream(qrStream, content, {
      type: imgExt,
      width: size,
      errorCorrectionLevel: errType,
    });

    const qrcodeGenerated = qrStream.pipe(res);
    res.write(qrcodeGenerated);
  } catch (err) {
    console.error("Failed to return content", err);
  }
};
