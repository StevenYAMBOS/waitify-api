// Constantes qu'on va réutiliser dans toute l'app

import dotenv from "dotenv";
dotenv.config();

// ============================================================================
// CODES DE STATUT HTTP
// ============================================================================

export const HTTP_STATUS = {
  // 2xx - Succès
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,

  // 4xx - Erreur client
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,

  // 5xx - Erreur serveur
  INTERNAL_SERVER_ERROR: 500,
} as const;

// ============================================================================
// MÉTHODES HTTP
// ============================================================================

export const HTTP_METHODS = {
  GET: "GET",
  POST: "POST",
  PUT: "PUT",
  PATCH: "PATCH",
  DELETE: "DELETE",
} as const;

// ============================================================================
// AUTHENTIFICATION & TOKENS
// ============================================================================

export const AUTH = {
  BEARER_PREFIX: "Bearer ",
  HEADER_NAME: "Authorization",
  // Messages
  INVALID_TOKEN: "Token de connexion invalide ou expiré",
  UNAUTHORIZED_ACCESS: "Accès non autorisé à cette ressource",
  MISSING_TOKEN: "Token d'authentification manquant",
  INVALID_CREDENTIALS: "Email ou mot de passe incorrect",
  EXPIRATION_TIME: "1h",
} as const;

// ============================================================================
// FILE D'ATTENTE - STATUTS
// ============================================================================

export const QUEUE_STATUSES = {
  WAITING: "waiting",
  CALLED: "called",
  SERVED: "served",
  MISSED: "missed",
  CANCELLED: "cancelled",
} as const;

// ============================================================================
// FILE D'ATTENTE - MESSAGES CLIENT
// ============================================================================

export const QUEUE_MESSAGES = {
  // Succès
  JOINED_SUCCESSFULLY: "Vous avez été ajouté à la file d'attente avec succès",
  POSITION_UPDATED: "Votre position dans la file est à jour",
  CANCELLED_SUCCESSFULLY: "Votre place a été annulée avec succès",
  CLIENT_CALLED: "Client appelé avec succès",
  CLIENT_MARKED_SERVED: "Client marqué comme servi avec succès",

  // Erreurs
  ALREADY_IN_QUEUE: "Vous êtes déjà inscrit dans la file d'attente",
  QUEUE_CLOSED: "La file d'attente est actuellement fermée",
  QUEUE_FULL: "La file d'attente est complète, impossible de s'inscrire",
  QUEUE_PAUSED: "La file d'attente est en pause",
  NO_CLIENTS_WAITING: "Aucun client en attente",
  ENTRY_NOT_FOUND: "Entrée non trouvée ou déjà traitée",
  ENTRY_NOT_CALLED: "Entrée introuvable ou pas encore appelée",
} as const;

// ============================================================================
// ÉTABLISSEMENTS
// ============================================================================

export const BUSINESS_MESSAGES = {
  NOT_FOUND: "L'établissement est introuvable ou inactif",
  NOT_FOUND_OR_INACTIVE: "Le commerce est introuvable ou inactif",
  NO_BUSINESSES: "Aucun établissement trouvé",
  CREATION_SUCCESS: "Établissement créé avec succès",
  UPDATE_SUCCESS: "Établissement mis à jour avec succès",
  DELETION_SUCCESS: "Établissement supprimé avec succès",
  MAX_BUSINESSES_REACHED:
    "Nombre maximum d'établissements atteint selon votre plan",
} as const;

// ============================================================================
// UTILISATEURS
// ============================================================================

export const USER_MESSAGES = {
  NOT_FOUND: "Utilisateur introuvable",
  CREATION_SUCCESS: "Utilisateur créé avec succès",
  UPDATE_SUCCESS: "Profil mis à jour avec succès",
  DELETION_SUCCESS: "Compte supprimé avec succès",
  EMAIL: "Email",
  EMAIL_ALREADY_EXISTS: "Cet email est déjà utilisé",
  LOGIN_SUCCESS: "Connexion réussie",
  LOGOUT_SUCCESS: "Déconnexion réussie",
  PROFILE_UPDATED: "Votre profil a été mis à jour",
  DOES_NOT_EXISTS: "L'utilisateur n'existe pas",
} as const;

// ============================================================================
// VALIDATION & FORMAT
// ============================================================================

export const VALIDATION = {
  // Champs requis
  PHONE_REQUIRED: "Le numéro de téléphone est requis",
  EMAIL_REQUIRED: "L'adresse email est requise",
  PASSWORD_REQUIRED: "Le mot de passe est requis",
  BUSINESS_ID_REQUIRED: "L'identifiant de l'établissement est requis",
  ENTRY_ID_REQUIRED: "L'identifiant de l'entrée est requis",
  NAME_REQUIRED: "Le nom est requis",

  // Format invalide
  INVALID_PHONE_FORMAT:
    "Le format du numéro de téléphone est invalide (format français attendu: +33 ou 0)",
  INVALID_EMAIL_FORMAT: "Le format de l'adresse email est invalide",
  INVALID_UUID_FORMAT: "Le format de l'identifiant est invalide",
  PASSWORD_TOO_WEAK:
    "Le mot de passe doit contenir au moins 8 caractères, une majuscule et un chiffre",
  INVALID_PASSWORD: "Le mot de passe est incorrect",
  // Autres
  MIN_LENGTH: "La longueur minimale requise n'est pas atteinte",
  MAX_LENGTH: "La longueur maximale a été dépassée",
} as const;

// ============================================================================
// MESSAGES D'ERREUR GÉNÉRIQUES
// ============================================================================

export const ERROR_MESSAGES = {
  INTERNAL_SERVER_ERROR: "Une erreur interne est survenue",
  INVALID_REQUEST: "Requête invalide",
  UNKNOWN_ERROR: "Erreur inconnue",
  DATABASE_ERROR: "Erreur lors de l'accès à la base de données",
  EXTERNAL_SERVICE_ERROR:
    "Erreur lors de la communication avec un service externe",
  TIMEOUT_ERROR: "La requête a dépassé le délai imparti",
  METHOD_NOT_ALLOWED: "Méthode HTTP non autorisée",
  INVALID_EMAIL_OR_PASSWORD: "Email ou mot de passe invalide",
} as const;

// ============================================================================
// GOOGLE API
// ============================================================================

export const GOOGLE_API = {
  CLIENT_ID: process.env.GCP_CLIENT_ID,
  CLIENT_NAME: process.env.GCP_CLIENT_NAME,
  CLIENT_SECRET: process.env.GCP_CLIENT_SECRET,
  REDIRECT_URL: process.env.GCP_CLIENT_CALLBACK,
  STORAGE_BUCKET_NAME: process.env.GCS_BUCKET_NAME,
  STORAGE_BUCKET: process.env.GCS_BUCKET,
} as const;

// ============================================================================
// ROUTES - AUTHENTIFICATION
// ============================================================================

export const ROUTES_AUTH = {
  BASE: "/auth",
  REGISTER: "/register",
  LOGIN: "/login",
  LOGOUT: "/logout",
  REFRESH_TOKEN: "/refresh",
  PROTECTED: "/protected",
} as const;

// ============================================================================
// ROUTES - UTILISATEURS
// ============================================================================

export const ROUTES_USERS = {
  BASE: "/users",
  BY_ID: "/:id",
  PROFILE: "/profile",
  SETTINGS: "/settings",
} as const;

// ============================================================================
// ROUTES - ÉTABLISSEMENTS
// ============================================================================

export const ROUTES_BUSINESSES = {
  BASE: "/businesses",
  BY_ID: "/:id",
  QUEUE: "/:id/queue",
  GENERATE_QR: "/:id/qr-code",
  SETTINGS: "/:id/settings",
  STATS: "/:id/stats",
} as const;

// ============================================================================
// ROUTES - FILES D'ATTENTE
// ============================================================================

export const ROUTES_QUEUES = {
  BASE: "/queues",
  BY_TOKEN: "/q/:token",
  JOIN: "/join",
  STATUS: "/:entryId/status",
  CANCEL: "/:entryId/cancel",
  NEXT_CLIENT: "/:businessId/next",
  MARK_SERVED: "/:entryId/served",
  STATS: "/:businessId/stats",
  HISTORY: "/:businessId/history",
} as const;

// ============================================================================
// ROUTES - SMS & NOTIFICATIONS
// ============================================================================

export const ROUTES_SMS = {
  BASE: "/sms",
  LOGS: "/logs",
  QUOTA: "/quota",
  SEND_TEST: "/send-test",
} as const;

// ============================================================================
// CONSTANTES MÉTIER
// ============================================================================

export const BUSINESS_CONFIG = {
  // Tailles de file par défaut
  DEFAULT_MAX_QUEUE_SIZE: 50,
  MIN_QUEUE_SIZE: 1,
  MAX_QUEUE_SIZE: 200,

  // Timeouts (en minutes)
  DEFAULT_CLIENT_TIMEOUT: 5,
  MIN_TIMEOUT: 1,
  MAX_TIMEOUT: 30,

  // Temps de service (en secondes)
  DEFAULT_SERVICE_TIME: 300,
  MIN_SERVICE_TIME: 60,
  MAX_SERVICE_TIME: 7200,

  // Plans d'abonnement
  TRIAL_DURATION_DAYS: 14,
  PLAN_BASIC_BUSINESSES: 1,
  PLAN_PRO_BUSINESSES: 5,
  PLAN_ENTERPRISE_BUSINESSES: -1, // Illimité

  // Quotas SMS
  SMS_QUOTA_BASIC: 1000,
  SMS_QUOTA_PRO: 2500,
  SMS_QUOTA_ENTERPRISE: 5000,
  SMS_COST_CENTS: 3,
} as const;

// ============================================================================
// SMS - TYPES DE MESSAGES
// ============================================================================

export const SMS_MESSAGE_TYPES = {
  CONFIRMATION: "confirmation",
  REMINDER: "reminder",
  YOUR_TURN: "your_turn",
  MISSED: "missed",
  CANCELLED: "cancelled",
  QUEUE_UPDATE: "queue_update",
} as const;

// ============================================================================
// SMS - TEMPLATES
// ============================================================================

export const SMS_TEMPLATES = {
  CONFIRMATION: (businessName: string, position: number, waitTime: number) =>
    `Votre place #${position} chez ${businessName} est confirmée. Temps d'attente estimé: ~${waitTime}min. Rescannez le QR code pour suivre votre position.`,

  REMINDER: (businessName: string, clientsAhead: number) =>
    `Rappel: Plus que ${clientsAhead} client${
      clientsAhead > 1 ? "s" : ""
    } devant vous chez ${businessName}.`,

  YOUR_TURN: (businessName: string) =>
    `C'est votre tour chez ${businessName}! Présentez-vous au comptoir maintenant.`,

  MISSED: (businessName: string) =>
    `Votre tour chez ${businessName} est passé. Rescannez le QR code pour vous réinscrire.`,

  CANCELLED: (businessName: string) =>
    `Votre place chez ${businessName} a été annulée.`,
} as const;

// ============================================================================
// RESSOURCES STATIQUES
// ============================================================================

export const ASSETS = {
  PLACEHOLDER_IMAGE:
    "https://media.istockphoto.com/id/985915172/fr/vectoriel/%C3%A9checs-de-checker-vecteur-abstrait-sans-soudure.jpg?s=612x612&w=0&k=20&c=4BLWcNYZe9uykbirGZHc2_0zZC0pIIKS4Tvt19oj8TQ=",
  WAITIFY_URL: "https://waitify.fr",
  APP_LOGO: "https://cdn.waitify.fr/logo.png",
} as const;

// ============================================================================
// TESTS
// ============================================================================

export const TEST_DATA = {
  // Utilisateur de test
  TEST_USER_EMAIL: "usertest@yopmail.com",
  TEST_USER_PASSWORD: "@Password1",

  // Messages de test
  TEST_LOGIN_SHOULD_SUCCEED:
    "Le test devrait connecter l'utilisateur et renvoyer un code 200",
  TEST_LOGIN_MESSAGE: "L'utilisateur est connecté",
} as const;

// ============================================================================
// CONFIGURATION GÉNÉRALE
// ============================================================================

export const APP_CONFIG = {
  TIMEZONE: "Europe/Paris",
  DEFAULT_LOCALE: "fr-FR",
  MAX_REQUEST_TIMEOUT_MS: 30000,
  RATE_LIMIT_WINDOW_MS: 900000, // 15 minutes
  RATE_LIMIT_MAX_REQUESTS: 100,
} as const;
