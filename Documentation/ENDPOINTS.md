# Documentation des routes API

Modifié le : 19/03/2026

Par : [Steven YAMBOS](www.linkedin.com/in/steven-yambos)

## Routes

### Routes d'authentification

#### Route d'inscription

Route : `/api/auth/register/`

Services utilisés :

- NeonDB.
- Azure Blob Storage.

Logique :

- L'utilisateur (les commerçants `Role = Owner`) s'inscrit avec :
    - Prénom
    - Nom de famille
    - Photo de profil
    - Email
- Ses informations sont enrgistrées en base de données (`NeonDB`).
- Sa photo de profil (optionnelle `ProfilePicture`) est enregistré sur Microsoft Azure Blob Storage dans -> `waitify` (storage account) -> `images` (conteneur) -> `<FirstnameLastname>` (dossier utilisateur). L'image a un `Contentype: image/webp` par defaut.

#### Route de connexion

#### Route de connexion (Google)
