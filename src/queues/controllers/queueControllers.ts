import { Request, Response } from "express";
import { pool } from "../../config/database";
import {
  BUSINESS_MESSAGES,
  ERROR_MESSAGES,
  HTTP_METHODS,
  HTTP_STATUS,
  QUEUE_MESSAGES,
  QUEUE_STATUSES,
  VALIDATION,
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
  if (req.method !== HTTP_METHODS.PATCH) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
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
      message: QUEUE_MESSAGES.POSITION_UPDATED,
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
  if (req.method !== HTTP_METHODS.POST) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
  }

  try {
    const { BusinessId, phone, clientName } = req.body;

    // Validations basiques
    if (!BusinessId || BusinessId.trim() === "") {
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.BUSINESS_ID_REQUIRED,
      });
      return;
    }

    if (!phone || phone.trim() === "") {
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.PHONE_REQUIRED,
      });
      return;
    }

    // Valider format téléphone français
    const phoneRegex = /^(\+33|0)[1-9]\d{8}$/;
    if (!phoneRegex.test(phone)) {
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.INVALID_PHONE_FORMAT,
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
      res.status(HTTP_STATUS.NOT_FOUND).json({
        error: BUSINESS_MESSAGES.NOT_FOUND_OR_INACTIVE,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: QUEUE_MESSAGES.QUEUE_CLOSED,
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
      QUEUE_STATUSES.WAITING,
    ];

    const duplicateResult = await pool.query(
      checkDuplicateQuery,
      duplicateValues
    );

    if (duplicateResult.rows[0].exists) {
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: QUEUE_MESSAGES.ALREADY_IN_QUEUE,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: QUEUE_MESSAGES.QUEUE_FULL,
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
      QUEUE_STATUSES.WAITING,
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
      message: QUEUE_MESSAGES.JOINED_SUCCESSFULLY,
      Entry: {
        id: entryId,
        BusinessID: businessId,
        phone,
        clientName,
        position,
        estimatedWaitTime,
        status: QUEUE_STATUSES.WAITING,
        createdAt: new Date(),
      },
    };

    res.status(HTTP_STATUS.CREATED).json(response);
  } catch (error: unknown) {
    console.error("Erreur JoinQueue : ", error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error:
        error instanceof Error ? error.message : ERROR_MESSAGES.UNKNOWN_ERROR,
    });
  }
};

// Récupérer les informations d'une file d'attente
export const GetQueueController = async (req: Request, res: Response) => {
  // Vérification méthode HTTP
  if (req.method !== HTTP_METHODS.POST) {
    res.status(HTTP_STATUS.BAD_REQUEST).send(ERROR_MESSAGES.METHOD_NOT_ALLOWED);
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

    res.status(HTTP_STATUS.OK).send(response);
  } catch (error: unknown) {
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.ENTRY_ID_REQUIRED,
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
      res.status(HTTP_STATUS.NOT_FOUND).json({
        error: QUEUE_MESSAGES.ENTRY_NOT_FOUND,
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
      status: QUEUE_STATUSES.WAITING,
    };

    res.status(HTTP_STATUS.OK).json(response);
  } catch (error: unknown) {
    console.error("Erreur GetQueueStatus : ", error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error:
        error instanceof Error ? error.message : ERROR_MESSAGES.UNKNOWN_ERROR,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.BUSINESS_ID_REQUIRED,
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

    const values: string[] = [BusinessId, QUEUE_STATUSES.WAITING];

    const clientResult = await pool.query(selectClientQuery, values);

    if (clientResult.rows.length === 0) {
      res.status(HTTP_STATUS.NOT_FOUND).json({
        error: QUEUE_MESSAGES.NO_CLIENTS_WAITING,
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
      message: QUEUE_MESSAGES.CLIENT_CALLED,
      Client: {
        id: clientId,
        phone,
        clientName: client_name,
        position,
      },
    };

    res.status(HTTP_STATUS.OK).json(response);
  } catch (error: unknown) {
    console.error("Erreur CallNextClient : ", error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error:
        error instanceof Error ? error.message : ERROR_MESSAGES.UNKNOWN_ERROR,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.ENTRY_ID_REQUIRED,
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
      res.status(HTTP_STATUS.NOT_FOUND).json({
        error: QUEUE_MESSAGES.ENTRY_NOT_FOUND,
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

    const values: (string | Date)[] = [
      id,
      QUEUE_STATUSES.CANCELLED,
      new Date(),
    ];

    await pool.query(cancelQuery, values);

    // Les triggers PostgreSQL recalculent automatiquement les positions des autres clients

    // TODO: Envoyer SMS de confirmation d'annulation
    // await sendSMS(phone, `Votre place chez [Business] a été annulée.`);

    // TODO: Enregistrer l'SMS dans sms_logs
    // await logSMSSent(id, phone, 'cancelled', ...);

    res.status(HTTP_STATUS.NO_CONTENT).send();
  } catch (error: unknown) {
    console.error("Erreur CancelQueueEntry : ", error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error:
        error instanceof Error ? error.message : ERROR_MESSAGES.UNKNOWN_ERROR,
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
      res.status(HTTP_STATUS.BAD_REQUEST).json({
        error: VALIDATION.ENTRY_ID_REQUIRED,
      });
      return;
    }

    // Vérifier que l'entrée existe et est en statut 'called'
    const selectQuery = `
      SELECT id, called_at, BusinessId
      FROM queue_entries
      WHERE id = $1 AND status = $2
    `;

    const values: string[] = [id, QUEUE_STATUSES.CALLED];

    const selectResult = await pool.query(selectQuery, values);

    if (selectResult.rows.length === 0) {
      res.status(HTTP_STATUS.NOT_FOUND).json({
        error: QUEUE_MESSAGES.ENTRY_NOT_CALLED,
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

    const updatedValues: (string | number | Date | null)[] = [
      id,
      serviceTime,
      QUEUE_STATUSES.SERVED,
      new Date(),
      new Date(),
    ];

    await pool.query(updateQuery, updatedValues);

    // Les triggers PostgreSQL recalculent automatiquement les positions des autres clients

    res.status(HTTP_STATUS.OK).json({
      message: QUEUE_MESSAGES.CLIENT_MARKED_SERVED,
      id,
    });
  } catch (error: unknown) {
    console.error("Erreur MarkClientAsServed : ", error);
    res.status(HTTP_STATUS.INTERNAL_SERVER_ERROR).json({
      message: ERROR_MESSAGES.INTERNAL_SERVER_ERROR,
      error:
        error instanceof Error ? error.message : ERROR_MESSAGES.UNKNOWN_ERROR,
    });
  }
};
