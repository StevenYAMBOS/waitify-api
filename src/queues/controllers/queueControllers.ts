import { Request, Response } from "express";
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
import { Queue } from "../models/queueModels";

/*
Activer ou désactiver la file d'attente
Côté Font on va envoyer un booléen (true ou false) pour activer ou désactiver la file d'attente
*/
export const ActivateQueueHandler = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const { isQueueActive} = req?.body;

    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params?.id;

    // Date du jour
    const now: Date = new Date();

    // Query
    const query: string = `UPDATE businesses SET is_queue_active=$2 WHERE id=$1 RETURNING *;`;
    // Valeurs
    const values: string[] = [
      idParam,
      isQueueActive,
      now,
    ];

    // Modifier les informations de la file d'attente en base de données
    const queue = await pool.query(query, values);
    const queueCreated: Queue = queue.rows[0];

    // Réponse
    res.status(OK).json(queueCreated);
  } catch (error: unknown) {
    console.error(INTERNAL_SERVER_ERROR_MESSAGE, error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }

// Récupérer les entreprises d'un utilisateur
export const GetUserBusinessesController = async (
  req: Request,
  res: Response
) => {
  // Vérification méthode HTTP
  if (req.method !== GET_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
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
    res.status(OK).send(businessesFetched);
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
