/*
Constantes qu'on va réutiliser dans toute l'app
*/

/* ++++++++++++++ Status codes ++++++++++++++ */
export const OK: number = 200;
export const CREATED: number = 201;
export const NO_CONTENT: number = 204;
export const BAD_REQUEST: number = 400;
export const UNAUTHORIZED: number = 401;
export const FORBIDDEN: number = 403;
export const NOT_FOUND: number = 404;
export const INTERNAL_SERVER_ERROR: number = 500;

/* ++++++++++++++ Méthodes HTTP ++++++++++++++ */
export const GET_METHOD: string = "GET";
export const POST_METHOD: string = "POST";
export const PUT_METHOD: string = "PUT";
export const PATCH_METHOD: string = "PATCH";
export const DELETE_METHOD: string = "DELETE";

/* ++++++++++++++ Messages ++++++++++++++ */
export const BEARER_STRING: string = `Bearer `;
export const EMPTY_STRING: string = ``;
export const AUTHORIZATION_HEADER: string = `Authorization`;
export const INVALID_TOKEN: string = `Token de connexion invalide ou expiré`;
export const BAD_HTTP_METHOD: string = `Mauvaise méthode HTTP`;
export const USER_NOT_FOUND: string = `Une erreur est survenue lors de la récupération des informations de l'utilisateur`;
export const BUSINESS_NOT_FOUND: string = `Une erreur est survenue lors de la récupération des informations de l'entreprise`;
export const UNAUTHORIZED_RESOURCE: string = `Accès non autorisé`;
export const INTERNAL_SERVER_ERROR_MESSAGE: string = `Une erreur interne est survenue`;
export const IMAGE: string =
  "https://media.istockphoto.com/id/985915172/fr/vectoriel/%C3%A9checs-de-checker-vecteur-abstrait-sans-soudure.jpg?s=612x612&w=0&k=20&c=4BLWcNYZe9uykbirGZHc2_0zZC0pIIKS4Tvt19oj8TQ=";
export const ALREADY_IN_QUEUE: string = `Vous êtes déjà dans la file d'attente`;
export const QUEUE_CLOSED: string = `La file d'attente est fermée`;
export const QUEUE_FULL: string = `File d'attente complète`;
export const JOIN_QUEUE_SUCCESS: string = `Vous avez été inscrit dans la file d'attente avec succès`;
export const BUSINESS_ID_REQUIRED: string = `L'identifiant du commerce est requis`;
export const PHONE_REQUIRED: string = `Le numéro de téléphone est requis`;
export const BUSINESS_NOT_FOUND_OR_INACTIVE: string = `Le commerce est introuvable ou inactif`;
export const INVALID_PHONE_FORMAT: string = `Le format du numéro de téléphone est invalide`;
export const QUEUE_STATUS_WAITING: string = `waiting`;
export const QUEUE_STATUS_MESSAGE: string = `Le status de la file d'attente est à présent à jour`;
export const NEXT_CLIENT_MESSAGE: string = `Le client est appelé`;
export const CANCELLED_CLIENT_STATUS: string = `cancelled`;
export const LOGIN_MESSAGE: string = `L'utilisateur est connecté !`;
export const ID_IS_MISSING: string = `ID requis`;
export const ENTRY_IS_MISSING: string = `Entrée non trouvée ou déjà traitée`;

/* ++++++++++++++ Routes ++++++++++++++ */
export const WAITIFY_URL: string = `https://waitify.fr`;
export const ID_PARAM: string = "/:id";
export const NEUTRAL_PATH: string = "/";
export const TEST_PATH: string = "/";
// Authentification
export const AUTH_PATH: string = "/auth";
export const REGISTER_PATH: string = "/register";
export const LOGIN_PATH: string = "/login";
export const PROTECTED_PATH: string = "/protected";
// Utilisateurs
export const USER_PATH: string = "/user";
export const PROFILE_PATH: string = "/profile";
// Entreprises
export const BUSINESS_PATH: string = "/businesses";
export const QRCODE_PATH: string = "/generate";
// Files d'attentes
export const QRCODE_TOKEN_PATH: string = "/q";
export const QUEUES_PATH: string = "/queues";
export const QUEUE_STATUS_PATH: string = "/status";
export const JOIN_QUEUE_PATH: string = "/join";
export const NEXT_CLIENT_PATH: string = "/next";
export const CANCEL_CLIENT_PATH: string = "/cancel";

/* ++++++++++++++ Tests ++++++++++++++ */
// Messages
export const TEST_LOGIN_RULE_MESSAGE: string = `Le test devrait connecter l'utilisateur et renvoyer un code 200`;
export const TEST_LOGIN_MESSAGE: string = `L'utilisateur est connecté`;
// Valeurs
export const TEST_LOGIN_EMAIL: string = `usertest@yopmail.com`;
export const TEST_LOGIN_PASSWORD: string = `@Password1`;
