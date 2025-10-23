/* 
Constantes qu'on va réutiliser dans toute l'app
*/

// Status codes
export const OK: number = 200;
export const CREATED: number = 201;
export const BAD_REQUEST: number = 400;
export const UNAUTHORIZED: number = 401;
export const NOT_FOUND: number = 404;
export const INTERNAL_SERVER_ERROR: number = 500;

// Méthodes HTTP
export const GET_METHOD: string = "GET";
export const POST_METHOD: string = "POST";
export const PUT_METHOD: string = "PUT";
export const PATCH_METHOD: string = "PATCH";
export const DELETE_METHOD: string = "DELETE";

// Messages
export const USER_NOT_FOUND: string = `User not found`;
export const BAD_HTTP_METHOD: string = `Mauvaise méthode HTTP`;
export const BUSINESS_NOT_FOUND: string = `Une erreur est survenue lors de la récupération des informations de l'entreprise`;
export const UNAUTHORIZED_RESOURCE: string = `You are not allowed to access this resource`;
