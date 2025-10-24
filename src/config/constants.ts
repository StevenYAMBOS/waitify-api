/* 
Constantes qu'on va réutiliser dans toute l'app
*/

/* ++++++++++++++ Status codes ++++++++++++++ */
export const OK: number = 200;
export const CREATED: number = 201;
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

/* ++++++++++++++ Routes ++++++++++++++ */
export const WAITIFY_URL: string = `https://waitify.fr`;
export const ID_PARAM: string = "/:id";
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
// Files d'attentes
export const QRCODE_TOKEN_PATH: string = "/q";
