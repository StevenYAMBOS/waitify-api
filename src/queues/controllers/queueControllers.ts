import { Request, Response } from "express";
import { pool } from "../../config/database";
import {
  ALREADY_IN_QUEUE,
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  BUSINESS_ID_REQUIRED,
  BUSINESS_NOT_FOUND_OR_INACTIVE,
  CREATED,
  INVALID_PHONE_FORMAT,
  INTERNAL_SERVER_ERROR,
  INTERNAL_SERVER_ERROR_MESSAGE,
  JOIN_QUEUE_SUCCESS,
  NOT_FOUND,
  OK,
  PHONE_REQUIRED,
  POST_METHOD,
  QUEUE_CLOSED,
  QUEUE_FULL,
  QUEUE_STATUS_WAITING,
  UNAUTHORIZED,
  PATCH_METHOD,
  QUEUE_STATUS_MESSAGE,
} from "../../config/constants";
import {
  GetQueueResponse,
  JoinQueueResponse,
  Queue,
  StatusQueueResponse,
} from "../models/queueModels";
import { v4 as uuidv4 } from "uuid";

/*
Activer ou désactiver la file d'attente
Côté Font on va envoyer un booléen (true ou false) pour activer ou désactiver la file d'attente
On utilise le token QR Code pour identifier l'entreprise plutôt que l'id
*/
export const ActivateQueueController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== PATCH_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const { isQueueActive } = req.body;

    // QR Code token récupéré depuis les paramètres de l'URL
    const idParam: string = req.params.id;

    // Date du jour
    // const now: Date = new Date();

    // Query
    const query: string = `UPDATE businesses SET is_queue_active=$2 WHERE qr_code_token=$1;`;

    // Valeurs
    const values: string[] = [idParam, isQueueActive];

    // Modifier les informations de la file d'attente en base de données
    await pool.query(query, values);

    const response: StatusQueueResponse = {
      message: QUEUE_STATUS_MESSAGE,
    };

    // Réponse
    res.status(OK).json(response);
  } catch (error: unknown) {
    console.error(INTERNAL_SERVER_ERROR_MESSAGE, error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }
};

// Rejoindre une file d'attente
export const JoinQueueController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    return res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const { businessId, phone, clientName } = req.body;

    /* ----- Validation des champs ----- */

    // Validation de l'identifiant du commerce
    if (!businessId) {
      return res.status(BAD_REQUEST).json({
        message: BUSINESS_ID_REQUIRED,
      });
    }

    // Validation du numéro de téléphone
    if (!phone) {
      return res.status(BAD_REQUEST).json({
        message: PHONE_REQUIRED,
      });
    }

    // Validation du format du téléphone (format français)
    const phoneRegex = /^(\+33|0)[1-9][0-9]{8}$/;
    if (!phoneRegex.test(phone)) {
      return res.status(BAD_REQUEST).json({
        message: INVALID_PHONE_FORMAT,
      });
    }

    // Date du jour
    const now: Date = new Date();

    // Vérifier que le commerce existe ET que la file d'attente est active
    const businessQuery: string = `SELECT is_queue_active, max_queue_size, average_service_time FROM businesses WHERE id = $1 AND is_active = true;`;
    const businessValues: string[] = [businessId];
    const businessResult = await pool.query(businessQuery, businessValues);

    // Vérifier si le commerce existe
    if (businessResult.rows.length === 0) {
      return res.status(NOT_FOUND).json({
        message: BUSINESS_NOT_FOUND_OR_INACTIVE,
      });
    }

    const { is_queue_active, max_queue_size, average_service_time } =
      businessResult.rows[0];

    // Vérifier que la file d'attente est active
    if (!is_queue_active) {
      return res.status(UNAUTHORIZED).json({
        message: QUEUE_CLOSED,
      });
    }

    // Vérifier que le client n'est pas déjà dans la file
    const alreadyInQueueQuery: string = `SELECT EXISTS(SELECT 1 FROM queue_entries WHERE BusinessId = $1 AND phone = $2 AND status = $3) as exists;`;
    const alreadyInQueueValues: string[] = [
      businessId,
      phone,
      QUEUE_STATUS_WAITING,
    ];
    const alreadyInQueueResult = await pool.query(
      alreadyInQueueQuery,
      alreadyInQueueValues
    );
    const isAlreadyInQueue: boolean = alreadyInQueueResult.rows[0].exists;

    if (isAlreadyInQueue) {
      return res.status(UNAUTHORIZED).json({
        message: ALREADY_IN_QUEUE,
      });
    }

    // Vérifier que la file n'est pas pleine
    const currentQueueSizeQuery: string = `SELECT COUNT(*)::integer as count FROM queue_entries WHERE BusinessId = $1 AND status = $2;`;
    const currentQueueSizeValues: string[] = [businessId, QUEUE_STATUS_WAITING];
    const currentQueueSizeResult = await pool.query(
      currentQueueSizeQuery,
      currentQueueSizeValues
    );
    const currentQueueSize: number = currentQueueSizeResult.rows[0].count;

    if (currentQueueSize >= max_queue_size) {
      return res.status(UNAUTHORIZED).json({
        message: QUEUE_FULL,
      });
    }

    // Position suivante
    const nextPosition: number = currentQueueSize + 1;

    // Calculer le temps d'attente estimé (en minutes)
    const estimatedWaitMinutes: number = Math.ceil(
      (currentQueueSize * average_service_time) / 60
    );

    // Insérer dans la base (le trigger SQL recalculera automatiquement les positions)
    const entryId: string = uuidv4();
    const insertQuery: string = `INSERT INTO queue_entries (id, BusinessId, phone, client_name, position, estimated_wait_time, status, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9) RETURNING *;`;
    const insertValues: (string | number | Date)[] = [
      entryId,
      businessId,
      phone,
      clientName || null,
      nextPosition,
      estimatedWaitMinutes,
      QUEUE_STATUS_WAITING,
      now,
      now,
    ];

    // Insertion des informations de l'utilisateur dans la file d'attente en base de données
    const insertResult = await pool.query(insertQuery, insertValues);
    const queueEntry = insertResult.rows[0];

    // TODO : Envoyer SMS de confirmation (à implémenter plus tard)
    // sendSMS()

    const response: JoinQueueResponse = {
      message: JOIN_QUEUE_SUCCESS,
      Entry: {
        id: queueEntry.id,
        BusinessID: queueEntry.BusinessId,
        phone: queueEntry.phone,
        clientName: queueEntry.client_name,
        position: queueEntry.position,
        estimatedWaitTime: queueEntry.estimated_wait_time,
        status: queueEntry.status,
        createdAt: queueEntry.created_at,
      },
    };

    // Réponse
    return res.status(CREATED).json(response);
  } catch (error: unknown) {
    console.error(INTERNAL_SERVER_ERROR_MESSAGE, error);
    return res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }
};

// Récupérer les informations d'une file d'attente
export const GetQueueController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Récupérer l'ID de l'entreprise depuis l'URL
    const idParam: string = req.params.id;

    // Valeur
    const value: string[] = [idParam];

    // Query
    const query: string = `SELECT id, BusinessId, phone, client_name, position, estimated_wait_time, status, called_at, served_at, actual_service_time, sms_sent_count, last_sms_sent_at, created_at, updated_at FROM queue_entries WHERE BusinessId = $1 AND status = 'waiting' ORDER BY position ASC`;

    // Message de succès
    const successMessage: string = "File d'attente récupérée avec succès.";

    // Récupérer les informations de la file d'attente en base de données
    const queue = await pool.query(query, value);

    // File d'attente récupérée
    const queueFetched: Queue[] = queue.rows[0];

    // Réponse
    const response: GetQueueResponse = {
      message: successMessage,
      queueLength: queueFetched.length,
      Queue: queueFetched,
    };

    res.status(OK).send(response);
  } catch (error: unknown) {
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }
};
