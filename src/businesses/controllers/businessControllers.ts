import { Request, Response } from "express";
import {
  Business,
  UpdateBusinessEntry,
  UpdateBusinessResponse,
} from "../models/businessModels";
import { pool } from "../../config/database";
import {
  ASSETS,
  ERROR_MESSAGES,
  HTTP_METHODS,
  HTTP_STATUS,
} from "../../config/constants";
import { v4 as uuidv4 } from "uuid";
import QRCode from "qrcode";
import { PassThrough } from "stream";

// Récupérer les informations d'une entreprise
export const GetBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.GET) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
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
    res.status(HTTP_STATUS.OK).send(businessFetched);
  } catch (error: unknown) {
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};

// Récupérer les entreprises d'un utilisateur
export const GetUserBusinessesController = async (
  req: Request,
  res: Response
) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.GET) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params?.id;
    // Pagination
    // const { page, size } = req.query;
    // const query: string = `SELECT * FROM businesses ORDER BY "businesses"."id" LIMIT $2 OFFSET (($1 - 1) * $2) WHERE UserId = $1`;
    // Query
    const query: string = `SELECT * FROM businesses WHERE UserId = $1`;
    // Valeur
    const value: string[] = [idParam];
    // Récupérer les informations de l'entreprise
    const response = await pool.query(query, value);

    // Entreprise récupérée
    const businessesFetched: Business[] = response?.rows;
    res.status(HTTP_STATUS.OK).send(businessesFetched);
  } catch (error: unknown) {
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};

//  Créer une entreprise + générer le QR Code
export const AddBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.POST) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
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
    } = req.body;
    // Générer l'id de l'entreprise + le token du QR Code
    const uuid: string = uuidv4();

    // Générer l'id de l'entreprise + le token du QR Code
    const qrCodeToken: string = uuidv4();

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
    const content: string = ASSETS.WAITIFY_URL + "/q/" + qrCodeToken;
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
    console.error(ERROR_MESSAGES.INTERNAL_SERVER_ERROR, error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};

//  Modifier les informations d'une entreprise existante
export const UpdateBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.PATCH) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    // Corps de la requête
    const { name, businessType, phoneNumber, address, city, zipCode, country } =
      req.body;
    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params?.id;
    // Date du jour
    const now: Date = new Date();
    // Message
    const message: string =
      "Informations de l'entreprise mise à jour avec succès !";

    // Query
    const query: string = `UPDATE businesses SET name = $2, business_type = $3, phone_number = $4, address = $5, city = $6, zip_code = $7, country = $8, updated_at = $9 WHERE id = $1`;

    // Valeurs
    const values: string[] = [
      idParam,
      name,
      businessType,
      phoneNumber,
      address,
      city,
      zipCode,
      country,
      now,
    ];

    // Insérer les informations de l'entreprise en base de données
    await pool.query(query, values);

    // Informations récupérées
    const businessUpdated: UpdateBusinessEntry = {
      name: name,
      businessType: businessType,
      phoneNumber: phoneNumber,
      address: address,
      city: city,
      zipCode: zipCode,
      country: country,
      updatedAt: now,
    };

    // Réponse envoyé au client
    const response: UpdateBusinessResponse = {
      message: message,
      Business: businessUpdated,
    };

    // Réponse
    res.status(HTTP_STATUS.OK).json(response);
  } catch (error: unknown) {
    console.error(ERROR_MESSAGES.INTERNAL_SERVER_ERROR, error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};

//  Supprimer une entreprise existante
export const DeleteBusinessController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.DELETE) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params?.id;
    // Message
    const message: string = `Entreprise supprimée avec succès`;

    // Query
    const query: string = `DELETE FROM businesses WHERE id = $1`;

    // Valeurs
    const values: string[] = [idParam];

    // Supprimer l'entreprise de la base de données
    await pool.query(query, values);

    // Réponse
    res.status(HTTP_STATUS.NO_CONTENT).json(message);
  } catch (error: unknown) {
    console.error(ERROR_MESSAGES.INTERNAL_SERVER_ERROR, error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};

// Générer un nouveau QRCode
// Le client (front) envoie le `qrCodeToken` dans sa requête (body) pour générer le QRCode
export const GenerateQRCodeController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.POST) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    // Corps de la requête
    const { qrCodeToken } = req.body;

    // Contenu du du QRCode
    const content: string = ASSETS.WAITIFY_URL + "/q/" + qrCodeToken;
    console.log("URL : ", content);

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

    // Envoie du QRCode dans la réponse (image `png` du QRCode)
    qrStream.pipe(res);
  } catch (error: unknown) {
    console.error(ERROR_MESSAGES.INTERNAL_SERVER_ERROR, error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error: error,
    });
  }
};
