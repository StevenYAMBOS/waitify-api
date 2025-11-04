import { Request, Response } from "express";
import { pool } from "../../config/database";
import {
  ALREADY_IN_QUEUE,
  BAD_HTTP_METHOD,
  BAD_REQUEST,
  CREATED,
  INTERNAL_SERVER_ERROR,
  INTERNAL_SERVER_ERROR_MESSAGE,
  JOIN_QUEUE_SUCCESS,
  OK,
  POST_METHOD,
  QUEUE_CLOSED,
  QUEUE_FULL,
  UNAUTHORIZED,
} from "../../config/constants";
import {
  GetQueueResponse,
  JoinQueueResponse,
  Queue,
} from "../models/queueModels";
import { v4 as uuidv4 } from "uuid";

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
    const { isQueueActive } = req.body;

    // id récupéré depuis les paramètres de l'URL
    const idParam: string = req.params.id;

    // Date du jour
    const now: Date = new Date();

    // Query
    const query: string = `UPDATE businesses SET is_queue_active=$2 WHERE id=$1 RETURNING *;`;
    // Valeurs
    const values: string[] = [idParam, isQueueActive, now];

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
};

// Rejoindre une file d'attente
export const JoinQueueHandler = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    // Corps de la requête
    const { BusinessId, phone, clientName } = req.body;

    /* ----- Validation des champs ----- */

    if (BusinessId === null) {
      res.status(BAD_REQUEST).send(INTERNAL_SERVER_ERROR_MESSAGE);
    }

    // Date du jour
    const now: Date = new Date();

    let IsQueueActive: boolean;
    let MaxQueueSize: number;
    let AverageServiceTime: number;

    // Vérifier que le commerce existe ET que la file d'attente est active
    const isBusinessActiveQuery: string = `SELECT is_queue_active, max_queue_size, average_service_time FROM businesses WHERE id = $1 AND is_active = true;`;
    const isBusinessActiveValues: string[] = [BusinessId];
    await pool.query(isBusinessActiveQuery, isBusinessActiveValues);
    if (!IsQueueActive) {
      res.status(UNAUTHORIZED).send(QUEUE_CLOSED);
    }

    // Vérifier que le client n'est pas déjà dans la file
    let alreadyInQueue: boolean;
    const alreadyInValues: string[] = [BusinessId, alreadyInQueue];
    const alreadyInQueueQuery: string = `SELECT EXISTS(SELECT 1 FROM queue_entries WHERE BusinessId = $1 AND phone = $2 AND status = 'waiting');`;
    await pool.query(alreadyInQueueQuery, alreadyInValues);
    if (alreadyInQueue === true) {
      res.status(UNAUTHORIZED).send(ALREADY_IN_QUEUE);
    }

    // Vérifier que la file n'est pas pleine
    let currentQueueSize: number;
    const currentQueueSizeValues: string[] = [BusinessId, currentQueueSize];
    const currentQueueSizeQuery: string = `SELECT COUNT(*) FROM queue_entries WHERE BusinessId = $1 AND status = 'waiting';`;
    await pool.query(currentQueueSizeQuery, currentQueueSizeValues);
    if (currentQueueSize >= MaxQueueSize) {
      res.status(UNAUTHORIZED).send(QUEUE_FULL);
      return;
    }

    // Position suivante
    const nextPosition: number = currentQueueSize + 1;

    // Calculer le temps d'attente estimé
    const estimatedWaitMinutes: number =
      (currentQueueSize * AverageServiceTime) / 60;

    // Insérer dans la base (le trigger SQL recalculera automatiquement les positions)
    const query = `INSERT INTO queue_entries (id, BusinessId, phone, client_name, position, estimated_wait_time, status, created_at, updated_at) ($1, $2,$3, $4, $5, $6, $7, $8, $9)`;
    const entryID: string = uuidv4();
    const status: string = "waiting";
    const values: string[] = [entryID, status];

    // Insertion des informations de l'utilisateur dans la file d'attente en base de données
    await pool.query(query, values);

    // const queue = await pool.query(query, values);
    // const queueCreated: QueueEntry = queue.rows[0];

    // TODO : Envoyer SMS de confirmation (à implémenter plus tard)
    // sendSMS()

    const response: JoinQueueResponse = {
      message: JOIN_QUEUE_SUCCESS,
      Entry: {
        id: entryID,
        BusinessID: BusinessId,
        phone: phone,
        clientName: clientName,
        position: nextPosition,
        estimatedWaitTime: estimatedWaitMinutes,
        status: status,
        createdAt: now,
      },
    };

    // Réponse
    res.status(CREATED).json(response);
  } catch (error: unknown) {
    console.error(INTERNAL_SERVER_ERROR_MESSAGE, error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error,
    });
  }
};

// Récupérer les informations d'une file d'attente
export const GetQueueHandler = async (req: Request, res: Response) => {
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
