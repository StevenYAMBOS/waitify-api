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
  CLIENT_CALLED_MESSAGE,
  CANCELLED_CLIENT_STATUS,
  NO_CONTENT,
  ID_IS_MISSING,
  ENTRY_IS_MISSING,
  NO_CLIENT,
  UNKNOWN_ERROR,
  NOW,
} from "../../config/constants";
import {
  GetQueueResponse,
  GetQueueStatusResponse,
  JoinQueueResponse,
  NextClientResponse,
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
    //

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
    const { BusinessId, phone, clientName } = req.body;

    /* ----- Validation des champs ----- */

    // Validation de l'identifiant du commerce
    if (!BusinessId) {
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

    // Vérifier que le commerce existe ET que la file d'attente est active
    const businessQuery: string = `SELECT is_queue_active, max_queue_size, average_service_time FROM businesses WHERE id = $1 AND is_active = true;`;
    const businessValues: string[] = [BusinessId];
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
      BusinessId,
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
    const currentQueueSizeValues: string[] = [BusinessId, QUEUE_STATUS_WAITING];
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
    const id: string = uuidv4();
    const insertQuery: string = `INSERT INTO queue_entries (id, BusinessId, phone, client_name, position, estimated_wait_time, status, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9) RETURNING *;`;
    const insertValues: (string | number | Date)[] = [
      id,
      BusinessId,
      phone,
      clientName || null,
      nextPosition,
      estimatedWaitMinutes,
      QUEUE_STATUS_WAITING,
      NOW,
      NOW,
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

/**
 * Récupère le statut en temps réel d'un client dans la file d'attente
 * Recalcule l'estimation de temps d'attente basée sur sa position actuelle
 * Paramètres URL: id (UUID du client dans la queue)
 */
export const GetQueueStatusController = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const { id } = req.params;

    // Validation du paramètre
    if (!id || id.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: ID_IS_MISSING,
      });
      return;
    }

    // Récupérer l'entrée et ses infos de business en une seule requête
    const query = `
      SELECT 
        qe.id,
        qe.position,
        qe.BusinessId,
        qe.estimated_wait_time,
        qe.status,
        b.average_service_time
      FROM queue_entries qe
      JOIN businesses b ON qe.BusinessId = b.id
      WHERE qe.id = $1 AND qe.status = 'waiting'
    `;

    const result = await pool.query(query, [id]);

    if (result.rows.length === 0) {
      res.status(NOT_FOUND).json({
        error: ENTRY_IS_MISSING,
      });
      return;
    }

    const { position, average_service_time } = result.rows[0];

    // Recalcul du temps d'attente en temps réel
    // Formule: (position - 1) * temps moyen d'un service / 60 pour convertir en minutes
    const currentEstimateMinutes = Math.ceil(
      ((position - 1) * average_service_time) / 60
    );

    const response: GetQueueStatusResponse = {
      position,
      estimatedWaitMinutes: currentEstimateMinutes,
      status: QUEUE_STATUS_WAITING,
    };

    res.status(OK).json(response);
  } catch (error: unknown) {
    console.error("Erreur GetQueueStatus : ", error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error instanceof Error ? error.message : UNKNOWN_ERROR,
    });
  }
};

/**
 * Appelle le client suivant en attente dans la file d'attente
 * Change le statut de 'waiting' à 'called' et enregistre l'heure d'appel
 * Les triggers PostgreSQL recalculent automatiquement les positions
 * Paramètres URL: BusinessId (UUID du business)
 * Body: vide (le premier client 'waiting' est appelé)
 */
export const CallNextClientController = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const { BusinessId } = req.params;

    // Validation du paramètre
    if (!BusinessId || BusinessId.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: ID_IS_MISSING,
      });
      return;
    }

    // Récupérer le premier client en attente (position 1, ordre FIFO)
    const selectClientQuery = `
      SELECT id, phone, client_name, position
      FROM queue_entries
      WHERE BusinessId = $1 AND status = 'waiting'
      ORDER BY position ASC
      LIMIT 1
    `;

    const values: string[] = [BusinessId, QUEUE_STATUS_WAITING];

    const clientResult = await pool.query(selectClientQuery, values);

    if (clientResult.rows.length === 0) {
      res.status(NOT_FOUND).json({
        error: NO_CLIENT,
      });
      return;
    }

    const { id: clientId, phone, client_name, position } = clientResult.rows[0];

    // Mettre à jour le statut du client à 'called'
    // NOW() avec timezone pour tracer précisément quand l'appel s'est fait
    const updateStatusQuery = `
      UPDATE queue_entries
      SET status = 'called', called_at = NOW(), updated_at = NOW()
      WHERE id = $1
      RETURNING id, phone, client_name, position
    `;

    await pool.query(updateStatusQuery, [clientId]);

    // Les triggers PostgreSQL recalculent automatiquement les positions des autres clients
    // Pas besoin de logique manuelle ici

    // TODO: Envoyer SMS "C'est votre tour !"
    // await sendSMS(phone, `C'est votre tour ! Présentez-vous au comptoir.`);

    // TODO: Enregistrer l'SMS dans sms_logs
    // await logSMSSent(clientId, phone, 'your_turn', ...);

    const response: NextClientResponse = {
      message: CLIENT_CALLED_MESSAGE,
      Client: {
        id: clientId,
        phone,
        clientName: client_name,
        position,
      },
    };

    res.status(OK).json(response);
  } catch (error: unknown) {
    console.error("Erreur CallNextClient : ", error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error instanceof Error ? error.message : UNKNOWN_ERROR,
    });
  }
};

/**
 * Annule la place d'un client dans la file d'attente
 * Change le statut de 'waiting' à 'cancelled'
 * Les triggers PostgreSQL recalculent automatiquement les positions
 * Paramètres URL: id (UUID du client)
 */
export const CancelQueueEntryController = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const { id } = req.params;

    // Validation du paramètre
    if (!id || id.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: ID_IS_MISSING,
      });
      return;
    }

    // Vérifier que l'entrée existe et est en attente
    const selectQuery = `
      SELECT id, phone, client_name, position, BusinessId
      FROM queue_entries
      WHERE id = $1 AND status = 'waiting'
    `;

    const selectResult = await pool.query(selectQuery, [id]);

    if (selectResult.rows.length === 0) {
      res.status(NOT_FOUND).json({
        error: ENTRY_IS_MISSING,
      });
      return;
    }

    // const { phone, client_name } = selectResult.rows[0];

    // Annuler la place du client
    const cancelQuery = `
      UPDATE queue_entries
      SET status = $2, updated_at = $3
      WHERE id = $1
      RETURNING id, status
    `;

    const values: (string | Date)[] = [id, CANCELLED_CLIENT_STATUS, NOW];

    await pool.query(cancelQuery, values);

    // Les triggers PostgreSQL recalculent automatiquement les positions des autres clients

    // TODO: Envoyer SMS de confirmation d'annulation
    // await sendSMS(phone, `Votre place chez [Business] a été annulée.`);

    // TODO: Enregistrer l'SMS dans sms_logs
    // await logSMSSent(id, phone, 'cancelled', ...);

    res.status(NO_CONTENT).send();
  } catch (error: unknown) {
    console.error("Erreur CancelQueueEntry : ", error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error instanceof Error ? error.message : UNKNOWN_ERROR,
    });
  }
};
