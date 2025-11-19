import { Request, Response } from "express";
import { pool } from "../../config/database";
import {
  ALREADY_IN_QUEUE,
  BAD_HTTP_METHOD,
  BAD_REQUEST,
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
  CALLED_CLIENT_STATUS,
  ENTRY_NOT_CALLED,
  SERVED_CLIENT_STATUS,
  SERVED_CLIENT_MESSAGE,
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

/**
 * Rejoindre la file d'attente d'un établissement
 * Crée une nouvelle entrée dans queue_entries avec validations métier
 * Les triggers PostgreSQL calculent automatiquement la position
 * Body: {
 *   BusinessId: string,    // Id du commerce
 *   phone: string,            // Numéro téléphone du client
 *   clientName?: string       // Nom du client (optionnel)
 * }
 */
export const JoinQueueController = async (
  req: Request,
  res: Response
): Promise<void> => {
  // Vérification méthode HTTP
  if (req.method !== POST_METHOD) {
    res.status(BAD_REQUEST).send(BAD_HTTP_METHOD);
  }

  try {
    const { BusinessId, phone, clientName } = req.body;

    // Validations basiques
    if (!BusinessId || BusinessId.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: ID_IS_MISSING,
      });
      return;
    }

    if (!phone || phone.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: PHONE_REQUIRED,
      });
      return;
    }

    // Valider format téléphone français
    const phoneRegex = /^(\+33|0)[1-9]\d{8}$/;
    if (!phoneRegex.test(phone)) {
      res.status(BAD_REQUEST).json({
        error: INVALID_PHONE_FORMAT,
      });
      return;
    }

    // Récupérer le business via le QR token
    const businessQuery = `
      SELECT id, is_queue_active, max_queue_size, average_service_time
      FROM businesses
      WHERE BusinessId = $1 AND is_active = $2
    `;

    const businessValues: (string | boolean)[] = [BusinessId, true];
    const businessResult = await pool.query(businessQuery, businessValues);

    // Vérifier si le commerce existe
    if (businessResult.rows.length === 0) {
      res.status(NOT_FOUND).json({
        error: BUSINESS_NOT_FOUND_OR_INACTIVE,
      });
      return;
    }

    const {
      id: businessId,
      is_queue_active,
      max_queue_size,
      average_service_time,
    } = businessResult.rows[0];

    // Vérification 1: File d'attente ouverte
    if (!is_queue_active) {
      res.status(BAD_REQUEST).json({
        error: QUEUE_CLOSED,
      });
      return;
    }

    // Vérification 2: Client pas déjà inscrit (même numéro + business + waiting)
    const checkDuplicateQuery = `
      SELECT EXISTS(
        SELECT 1 FROM queue_entries
        WHERE BusinessId = $1 AND phone = $2 AND status = $3
      ) as exists
    `;

    const duplicateValues: (string | number)[] = [
      businessId,
      phone,
      QUEUE_STATUS_WAITING,
    ];

    const duplicateResult = await pool.query(
      checkDuplicateQuery,
      duplicateValues
    );

    if (duplicateResult.rows[0].exists) {
      res.status(BAD_REQUEST).json({
        error: ALREADY_IN_QUEUE,
      });
      return;
    }

    // Vérification 3: File pas pleine
    const currentQueueQuery = `
      SELECT COUNT(*)::int as queue_count
      FROM queue_entries
      WHERE BusinessId = $1 AND status = $2
    `;

    const queueCountResult = await pool.query(currentQueueQuery, [businessId]);
    const currentQueueSize = queueCountResult.rows[0].queue_count;

    if (currentQueueSize >= max_queue_size) {
      res.status(BAD_REQUEST).json({
        error: QUEUE_FULL,
      });
      return;
    }

    // Calcul du temps d'attente estimé
    // Formula: (nombre_clients_avant * temps_service_moyen) / 60 pour avoir des minutes
    const estimatedWaitTime = Math.ceil(
      (currentQueueSize * average_service_time) / 60
    );

    // Créer l'entrée dans la file
    // Le trigger PostgreSQL calcule automatiquement la position
    const entryId = uuidv4();
    const insertQuery = `
      INSERT INTO queue_entries (
        id, BusinessId, phone, client_name, 
        estimated_wait_time, status, created_at, updated_at
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
      RETURNING id, estimated_wait_time, created_at
    `;

    const insertValues: (string | number)[] = [
      entryId,
      businessId,
      phone,
      clientName || null,
      estimatedWaitTime,
      QUEUE_STATUS_WAITING,
    ];

    await pool.query(insertQuery, insertValues);

    // Récupérer la position calculée par le trigger
    const positionQuery = `
      SELECT position FROM queue_entries WHERE id = $1
    `;

    const positionResult = await pool.query(positionQuery, [entryId]);
    const position = positionResult.rows[0].position;

    // TODO: Envoyer SMS de confirmation
    // await sendSMS(phone, `Votre place #${position} confirmée. Temps d'attente: ~${estimatedWaitTime}min`);

    // TODO: Enregistrer l'SMS dans sms_logs
    // await logSMSSent(entryId, phone, 'confirmation', ...);

    const response: JoinQueueResponse = {
      message: JOIN_QUEUE_SUCCESS,
      Entry: {
        id: entryId,
        BusinessID: businessId,
        phone,
        clientName,
        position,
        estimatedWaitTime,
        status: QUEUE_STATUS_WAITING,
        createdAt: NOW,
      },
    };

    res.status(CREATED).json(response);
  } catch (error: unknown) {
    console.error("Erreur JoinQueue : ", error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error instanceof Error ? error.message : UNKNOWN_ERROR,
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
      estimatedWaitTime: currentEstimateMinutes,
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

/**
 * Confirme que le client a été servi avec succès
 * Change le statut de 'called' à 'served' et enregistre l'heure de fin
 * Enregistre le temps de service réel pour améliorer les estimations futures
 * Les triggers PostgreSQL recalculent automatiquement les positions
 * Route: PATCH /queues/:id/served
 * Paramètres URL: id (UUID du client)
 * Body: { actualServiceTime?: number } (optionnel, en secondes)
 */
export const MarkClientAsServedController = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const { id } = req.params;
    const { actualServiceTime } = req.body;

    // Validation du paramètre
    if (!id || id.trim() === "") {
      res.status(BAD_REQUEST).json({
        error: ID_IS_MISSING,
      });
      return;
    }

    // Vérifier que l'entrée existe et est en statut 'called'
    const selectQuery = `
      SELECT id, called_at, BusinessId
      FROM queue_entries
      WHERE id = $1 AND status = $2
    `;

    const values: string[] = [id, CALLED_CLIENT_STATUS];

    const selectResult = await pool.query(selectQuery, values);

    if (selectResult.rows.length === 0) {
      res.status(NOT_FOUND).json({
        error: ENTRY_NOT_CALLED,
      });
      return;
    }

    // Calculer le temps de service réel si fourni, sinon NULL
    const serviceTime =
      actualServiceTime && actualServiceTime > 0 ? actualServiceTime : null;

    // Marquer le client comme servi
    const updateQuery = `
      UPDATE queue_entries
      SET status = $3, served_at = $4, 
          actual_service_time = $2, updated_at = $5
      WHERE id = $1
      RETURNING id, status
    `;

    const updatedValues: string[] = [
      id,
      serviceTime,
      SERVED_CLIENT_STATUS,
      NOW,
      NOW,
    ];

    await pool.query(updateQuery, updatedValues);

    // Les triggers PostgreSQL recalculent automatiquement les positions des autres clients

    res.status(OK).json({
      message: SERVED_CLIENT_MESSAGE,
      id,
    });
  } catch (error: unknown) {
    console.error("Erreur MarkClientAsServed : ", error);
    res.status(INTERNAL_SERVER_ERROR).json({
      message: INTERNAL_SERVER_ERROR_MESSAGE,
      error: error instanceof Error ? error.message : UNKNOWN_ERROR,
    });
  }
};
