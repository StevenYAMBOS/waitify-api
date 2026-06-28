# Documentation des routes API

Modifié le : 01/06/2026

Par : [Steven YAMBOS](www.linkedin.com/in/steven-yambos)

## Routes

### Routes entreprises

### Créer une entreprise

> Chemin : `POST /api/business`

#### Description

Cet endpoint permet à un utilisateur authentifié de créer une nouvelle entreprise associée à son compte. Il est destiné aux utilisateurs disposant d'un token JWT valide. La création génère automatiquement un QR code unique pointant vers la file d'attente de l'entreprise.

---

#### Requête HTTP

- **Méthode :** `POST`
- **Chemin :** `/api/business`
- **Authentification requise :** Oui (type : Bearer JWT)

##### Headers obligatoires

| Header          | Valeur                |
| --------------- | --------------------- |
| `Authorization` | `Bearer <token>`      |
| `Content-Type`  | `multipart/form-data` |

> ⚠️ Le body est envoyé en `multipart/form-data` (et non `application/json`) car il peut contenir un fichier image (`Logo`). Tout client HTTP doit utiliser ce content-type.

##### Headers optionnels

_Aucun header optionnel identifié dans le code._

---

#### Body (`multipart/form-data`)

| Champ          | Type        | Obligatoire | Contraintes                                                                             | Description                          |
| -------------- | ----------- | ----------- | --------------------------------------------------------------------------------------- | ------------------------------------ |
| `Name`         | `string`    | ✅ Oui      | —                                                                                       | Nom de l'entreprise.                 |
| `BusinessType` | `string`    | ✅ Oui      | —                                                                                       | Type / catégorie de l'entreprise.    |
| `PhoneNumber`  | `string`    | ✅ Oui      | —                                                                                       | Numéro de téléphone de l'entreprise. |
| `Address`      | `string`    | ✅ Oui      | —                                                                                       | Adresse postale.                     |
| `City`         | `string`    | ✅ Oui      | —                                                                                       | Ville.                               |
| `ZipCode`      | `string`    | ✅ Oui      | —                                                                                       | Code postal.                         |
| `Country`      | `string`    | ✅ Oui      | Valeur par défaut : `"France"`                                                          | Pays.                                |
| `Logo`         | `IFormFile` | ❌ Non      | Taille max : **1 Mo**. Extensions autorisées : `.jpeg`, `.jpg`, `.png`, `.webp`, `.svg` | Logo de l'entreprise.                |

> **Note :** Le champ `QrCodeToken` est présent dans le modèle `BusinessRequest` mais **n'est pas fourni par le client** : il est généré côté serveur (`Guid.NewGuid()`) lors de la création.

---

#### Comportement serveur

1. Extraction de l'ID utilisateur depuis le claim JWT `nameidentifier`.
2. Vérification de l'existence de l'utilisateur en base de données.
3. Si un logo est fourni :
   - Validation de la taille (≤ 1 Mo).
   - Upload vers Azure Blob Storage (conteneur défini par la variable d'environnement `AzureBlobBusinessesContainer`).
   - Validation de l'extension du fichier (`.jpeg`, `.jpg`, `.png`, `.webp`, `.svg`).
4. Création de l'entité `Business` en base de données avec un `QrCodeToken` unique (`Guid`).
5. Génération d'un QR code pointant vers l'URL : `{WaitifyUrl}/q/{QrCodeToken}`.
6. Retour du QR code généré.

---

#### Réponses

##### ✅ `200 OK` – Succès

Le QR code de l'entreprise créée est retourné sous forme de chaîne de caractères (format à préciser : base64, URL, SVG…).

```
<qr_code_data>
```

> ⚠️ Le type de retour exact (`string`) est issu de `CreateBusinessAsync`. Le format précis du QR code (base64, data URI, SVG, etc.) dépend de l'implémentation de `QRCodeHelper.GenerateQRCode` — [À compléter].

---

##### ❌ `400 Bad Request` – Fichier trop volumineux

Retourné si le logo dépasse 1 Mo.

```
La taille du fichier ne doit pas excéder 1MB.
```

---

##### ❌ `404 Not Found` – Utilisateur introuvable (claim JWT invalide)

Retourné si l'ID extrait du token JWT ne correspond à aucun utilisateur.

```
Utilisateur introuvable
```

---

##### ❌ `404 Not Found` – Échec de création

Retourné si `businessService.CreateBusinessAsync` retourne `null`.

```
Erreur lors de la création de l'entreprise.
```

---

##### ❌ `400 Bad Request` – Validation du modèle

Retourné automatiquement par ASP.NET si un champ obligatoire est absent ou invalide. Les messages d'erreur sont définis dans `BusinessRequest` :

| Champ          | Message d'erreur                                     |
| -------------- | ---------------------------------------------------- |
| `Name`         | `"Le nom est incorrecte."`                           |
| `BusinessType` | `"Le type est obligatoire."`                         |
| `PhoneNumber`  | `"Le format du numéro de téléphone est incorrecte."` |
| `Address`      | `"L'adresse est incorrecte."`                        |
| `City`         | `"La ville est obligatoire."`                        |
| `ZipCode`      | `"Le code postale est obligatoire."`                 |
| `Country`      | `"Le pays est obligatoire."`                         |

---

### Exemple de requête

```http
POST /api/business HTTP/1.1
Host: [À compléter]
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----FormBoundary

------FormBoundary
Content-Disposition: form-data; name="Name"

Ma Boulangerie
------FormBoundary
Content-Disposition: form-data; name="BusinessType"

Boulangerie
------FormBoundary
Content-Disposition: form-data; name="PhoneNumber"

+33612345678
------FormBoundary
Content-Disposition: form-data; name="Address"

12 Rue de la Paix
------FormBoundary
Content-Disposition: form-data; name="City"

Paris
------FormBoundary
Content-Disposition: form-data; name="ZipCode"

75001
------FormBoundary
Content-Disposition: form-data; name="Country"

France
------FormBoundary
Content-Disposition: form-data; name="Logo"; filename="logo.png"
Content-Type: image/png

<binary data>
------FormBoundary--
```

---

### Exemple de réponse (`200 OK`)

```
[À compléter – dépend du format retourné par QRCodeHelper.GenerateQRCode]
```

---

### Dépendances internes

| Composant                 | Rôle                                                                |
| ------------------------- | ------------------------------------------------------------------- |
| `TokenService`            | Extraction du claim `nameidentifier` depuis le JWT.                 |
| `ApplicationUserService`  | Vérification de l'existence de l'utilisateur (`FindUserByIdAsync`). |
| `FileStorageService`      | Upload du logo vers Azure Blob Storage (`UploadBlobAsync`).         |
| `QRCodeHelper`            | Génération du QR code (`GenerateQRCode`).                           |
| `AppDbContext`            | Persistance de l'entité `Business`.                                 |
| `AppConstants.WaitifyUrl` | URL de base utilisée pour construire le lien du QR code.            |

---

### Variables d'environnement requises

| Variable                       | Usage                                            |
| ------------------------------ | ------------------------------------------------ |
| `AzureBlobBusinessesContainer` | Nom du conteneur Azure Blob pour les logos.      |
| [À compléter]                  | `AppConstants.WaitifyUrl` – URL de base Waitify. |

---

### Notes

- L'endpoint utilise `[FromForm]` : le body doit impérativement être envoyé en `multipart/form-data`, même si aucun fichier n'est joint.
- Le `QrCodeToken` est généré côté serveur et ne doit pas être fourni par le client, bien qu'il soit présent dans le modèle `BusinessRequest` avec une annotation `[Required]` — incohérence à corriger côté code.
- La validation de l'extension du logo est effectuée côté service (`FileStorageService.UploadBlobAsync`) ; le comportement en cas d'extension non autorisée (exception, retour `null`, code HTTP) est [À compléter].

---

### Routes authentification Google (OAuth 2.0)

---

### Initier la connexion avec Google

> Chemin : `GET /api/auth/login/google`

#### Description

Déclenche le flux OAuth 2.0 avec Google. Le serveur construit les propriétés d'authentification externe via `SignInManager`, génère l'URL de callback vers `GET /api/auth/signin-google`, puis retourne un challenge HTTP qui redirige le client vers la page de consentement Google.

Aucun token n'est requis pour appeler cet endpoint. Le client doit pouvoir suivre les redirections HTTP (302).

---

#### Requête HTTP

- **Méthode :** `GET`
- **Chemin :** `/api/auth/login/google`
- **Authentification requise :** Non

##### Headers obligatoires

_Aucun header obligatoire._

##### Headers optionnels

_Aucun header optionnel identifié dans le code._

##### Paramètres de requête (Query string)

| Paramètre   | Type     | Obligatoire | Description                                                                                                                  |
| ----------- | -------- | ----------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `returnUrl` | `string` | Oui         | URL vers laquelle le client sera redirigé après authentification réussie. Doit être encodée en URL (`Uri.EscapeDataString`). |

##### Body

Aucun body attendu.

---

#### Comportement serveur

1. Construction de l'URL de callback : chemin de l'endpoint `GoogleLoginCallback` (`/api/auth/signin-google`) auquel est appendu `?returnUrl=<returnUrl encodée>`.
2. Configuration des propriétés d'authentification externe via `SignInManager.ConfigureExternalAuthenticationProperties("Google", callbackUrl)`.
3. Retour d'un `Challenge(properties, ["Google"])` → réponse HTTP `302` vers la page de consentement Google.

---

#### Réponses

##### `302 Found` – Redirection vers Google

Redirige le navigateur vers la page de consentement OAuth Google. Ce comportement est géré automatiquement par le middleware ASP.NET Identity/Google.

```
Location: https://accounts.google.com/o/oauth2/auth?...
```

> Il n'y a pas de réponse JSON pour cet endpoint. La réponse est toujours une redirection.

---

#### Exemple de requête

```http
GET /api/auth/login/google?returnUrl=https%3A%2F%2Fwaitify.fr%2Fdashboard HTTP/1.1
Host: [À compléter]
```

---

#### Dépendances internes

| Composant                        | Rôle                                                                                                  |
| -------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `SignInManager<ApplicationUser>` | Construction des propriétés d'authentification externe (`ConfigureExternalAuthenticationProperties`). |
| `LinkGenerator`                  | Résolution de l'URL de callback à partir du nom d'endpoint `GoogleLoginCallback`.                     |

---

#### Variables d'environnement requises

| Variable                       | Usage                       |
| ------------------------------ | --------------------------- |
| `AuhtenticationGoogleClientId` | Client ID OAuth Google.     |
| `AuhtenticationGoogleSecret`   | Client Secret OAuth Google. |

---

#### Notes

- Cet endpoint ne produit aucun token JWT directement. Il initie uniquement le flux OAuth.
- Le paramètre `returnUrl` n'est pas validé côté serveur : aucune vérification de liste blanche n'est présente dans le code.
- L'endpoint est soumis au rate limiter `"fixed"` (`[EnableRateLimiting("fixed")]`) configuré au niveau du contrôleur.

---

### Callback OAuth Google

> Chemin : `GET /api/auth/signin-google`

#### Description

Endpoint de callback appelé automatiquement par Google après que l'utilisateur a accordé (ou refusé) l'accès. Le serveur authentifie la session via le schéma Cookie, crée ou retrouve l'utilisateur en base de données, enregistre le login externe Google, puis redirige vers l'URL fournie initialement.

Cet endpoint n'est pas destiné à être appelé directement par un client. Il est invoqué par le serveur OAuth Google à l'issue du flux de consentement.

---

#### Requête HTTP

- **Méthode :** `GET`
- **Chemin :** `/api/auth/signin-google`
- **Authentification requise :** Non (gérée en interne via le schéma Cookie posé par Google OAuth)

##### Headers obligatoires

_Aucun header obligatoire côté client. Les cookies de session OAuth sont gérés automatiquement par le navigateur._

##### Paramètres de requête (Query string)

| Paramètre   | Type     | Obligatoire | Description                                                                            |
| ----------- | -------- | ----------- | -------------------------------------------------------------------------------------- |
| `returnUrl` | `string` | Oui         | URL de redirection finale après succès. Transmise depuis `GET /api/auth/login/google`. |

> Google ajoute également ses propres paramètres (`code`, `state`, `scope`) à cette URL lors du callback. Ils sont consommés automatiquement par le middleware et ne doivent pas être fournis manuellement.

##### Body

Aucun body attendu.

---

#### Comportement serveur

1. Authentification de la requête via `HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme)`.
2. Si l'authentification échoue (`result.Succeeded == false`) → retourne `401 Unauthorized`.
3. Appel de `LoginWithGoogleAsync(result.Principal)` :
   a. Extraction du claim `ClaimTypes.Email` depuis le `ClaimsPrincipal`.
   b. Si l'email est `null` → lève `ExternalLoginProviderException`.
   c. Recherche de l'utilisateur en base par email (`FindByEmailAsync`).
   d. Si l'utilisateur **n'existe pas** : création d'un nouvel `ApplicationUser` avec les données suivantes :

   | Champ            | Source                                                                                                                       | Valeur par défaut |
   | ---------------- | ---------------------------------------------------------------------------------------------------------------------------- | ----------------- |
   | `UserName`       | `ClaimTypes.Email`                                                                                                           | —                 |
   | `Email`          | `ClaimTypes.Email`                                                                                                           | —                 |
   | `FirstName`      | `ClaimTypes.GivenName`                                                                                                       | `""` (vide)       |
   | `LastName`       | `ClaimTypes.Surname`                                                                                                         | `""` (vide)       |
   | `EmailConfirmed` | Fixé à `true`                                                                                                                | —                 |
   | `AuthProvider`   | Fixé à `"Google"`                                                                                                            | —                 |
   | `GoogleId`       | `ClaimTypes.NameIdentifier`                                                                                                  | `""` (vide)       |
   | `Role`           | Fixé à `Role.Owner`                                                                                                          | —                 |
   | `PhoneNumber`    | `ClaimTypes.HomePhone` ou `ClaimTypes.MobilePhone`                                                                           | `null`            |
   | `ProfilePicture` | URL construite : `https://people.googleapis.com/v1/people/{NameIdentifier}?personFields=photos&key=image&key={GoogleApiKey}` | —                 |
   | `TrialEndsAt`    | `DateTime.UtcNow + 360h (15 jours)`                                                                                          | —                 |
   | `CreatedAt`      | `DateTime.UtcNow`                                                                                                            | —                 |
   | `LastLogin`      | `DateTime.UtcNow`                                                                                                            | —                 |

   e. Si la création échoue → lève `ExternalLoginProviderException`.
   f. Ajout du login externe Google (`UserLoginInfo` avec `LoginProvider="Google"`, `ProviderKey=ClaimTypes.NameIdentifier`) si non déjà présent.
   g. Si l'ajout du login échoue → lève `ExternalLoginProviderException`.

4. Redirection vers `returnUrl`.

> **Note :** La génération de token JWT et de refresh token est actuellement **commentée** dans le code (`LoginWithGoogleAsync`). Aucun token n'est donc émis à l'issue de ce flux. Le mécanisme de session post-authentification Google est [À compléter].

---

#### Réponses

##### `302 Found` – Succès

Redirige le client vers `returnUrl` après création ou mise à jour de l'utilisateur.

```
Location: <returnUrl>
```

---

##### `401 Unauthorized` – Authentification Cookie échouée

Retourné si `HttpContext.AuthenticateAsync` ne réussit pas (cookie de session OAuth absent, expiré ou invalide).

```
HTTP/1.1 401 Unauthorized
```

> Aucun body JSON n'est retourné pour ce cas.

---

##### Erreurs internes (non exposées directement en HTTP)

Les cas suivants lèvent une `ExternalLoginProviderException` non interceptée dans le contrôleur :

| Condition                            | Message d'erreur interne                      |
| ------------------------------------ | --------------------------------------------- |
| `ClaimsPrincipal` est `null`         | `"ClaimsPrincipal est null"`                  |
| Claim `Email` absent du principal    | `"Email est null"`                            |
| Échec de `userManager.CreateAsync`   | `"Unable to create user: <détails Identity>"` |
| Échec de `userManager.AddLoginAsync` | `"Unable to login user: <détails Identity>"`  |

> Le comportement HTTP résultant de ces exceptions dépend de la gestion globale des erreurs de l'application — [À compléter].

---

#### Exemple de flux complet

```
1. Client → GET /api/auth/login/google?returnUrl=https%3A%2F%2Fwaitify.fr%2Fdashboard
2. Serveur → 302 vers https://accounts.google.com/o/oauth2/auth?...
3. Utilisateur consent sur Google
4. Google → GET /api/auth/signin-google?code=...&state=...&returnUrl=https%3A%2F%2Fwaitify.fr%2Fdashboard
5. Serveur → 302 vers https://waitify.fr/dashboard
```

---

#### Dépendances internes

| Composant                          | Rôle                                                                              |
| ---------------------------------- | --------------------------------------------------------------------------------- |
| `SignInManager<ApplicationUser>`   | Authentification via le schéma Cookie (`AuthenticateAsync`).                      |
| `AuthService.LoginWithGoogleAsync` | Création/récupération de l'utilisateur et enregistrement du login externe Google. |
| `UserManager<ApplicationUser>`     | Recherche, création et gestion des logins externes de l'utilisateur.              |

---

#### Variables d'environnement requises

| Variable                       | Usage                                                             |
| ------------------------------ | ----------------------------------------------------------------- |
| `GoogleApiKey`                 | Clé API Google utilisée pour construire l'URL de photo de profil. |
| `AuhtenticationGoogleClientId` | Client ID OAuth Google (configuré au démarrage).                  |
| `AuhtenticationGoogleSecret`   | Client Secret OAuth Google (configuré au démarrage).              |

---

#### Notes

- Cet endpoint est nommé `GoogleLoginCallback` via `[EndpointName("GoogleLoginCallback")]`, ce qui permet à `LinkGenerator` de résoudre son URL depuis `GoogleLogin()`.
- La photo de profil stockée dans `ProfilePicture` est une URL d'API Google People (non une image directe) et nécessite une clé API valide pour être utilisée.
- L'émission de token JWT post-authentification Google est actuellement désactivée (code commenté). L'intégration front-end ne peut pas récupérer de JWT à l'issue de ce flux en l'état.
- Aucun email de bienvenue n'est envoyé lors d'une inscription via Google (contrairement au flux `POST /api/auth/register`).

---

### Routes file d'attente

---

### `POST /api/queue/join` – Rejoindre une file d'attente

#### Description

Permet à un client d'intégrer la file d'attente d'une entreprise en scannant son QR code. L'endpoint est destiné aux clients finaux (public non authentifié). Avant d'inscrire le client, le serveur vérifie que la file est ouverte, que le numéro de téléphone n'est pas déjà présent dans la file et que la capacité maximale n'est pas atteinte.

---

#### Requête HTTP

- **Méthode :** `POST`
- **Chemin :** `/api/queue/join`
- **Authentification requise :** Non

##### Headers obligatoires

| Header         | Valeur             |
| -------------- | ------------------ |
| `Content-Type` | `application/json` |

##### Headers optionnels

_Aucun header optionnel identifié dans le code._

##### Body (`application/json`)

| Champ         | Type     | Obligatoire | Contraintes                  | Description                                                          |
| ------------- | -------- | ----------- | ---------------------------- | -------------------------------------------------------------------- |
| `qrCodeToken` | `Guid`   | ✅ Oui      | UUID valide                  | Identifiant QR code de l'entreprise, lu depuis le QR scanné.         |
| `phone`       | `string` | ✅ Oui      | Format téléphone (`[Phone]`) | Numéro de téléphone du client. Doit être unique dans la file active. |
| `clientName`  | `string` | ❌ Non      | —                            | Nom affiché du client.                                               |

```json
{
  "qrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phone": "+33612345678",
  "clientName": "Jean Dupont"
}
```

---

#### Comportement serveur

1. Recherche de l'entreprise correspondant au `qrCodeToken` via `FindBusinessByQrTokenAsync`.
2. Si aucune entreprise trouvée → `404 Not Found`.
3. Si `business.IsQueueActive == false` → `400 Bad Request` (file fermée).
4. Vérification de l'unicité du numéro de téléphone : si `phone` est déjà présent dans la file de cette entreprise avec le statut `"waiting"` → `400 Bad Request`.
5. Comptage des clients en attente (`status == "waiting"`) pour cette entreprise.
6. Si `waitingCount >= business.MaxQueueSize` → `400 Bad Request` (file pleine).
7. Calcul du temps d'attente estimé : `estimatedWaitTime = (waitingCount × business.AverageServiceTime) / 60` (résultat en minutes, division entière).
8. Calcul de la position via `QueuePositionHelper.CalculateNewPosition(waitingCount)`.
9. Création de l'entrée `QueueEntries` avec le statut `"waiting"` et persistance en base.
10. Retour de `JoinQueueResponse`.

> **Note :** `business.AverageServiceTime` est exprimé en secondes (valeur par défaut : `300` s). Le temps estimé retourné est en minutes.

---

#### Réponses

##### ✅ `200 OK` – Inscription réussie

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "businessQrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "businessName": "Ma Boulangerie",
  "position": 3,
  "estimatedWaitTime": 10,
  "phone": "+33612345678",
  "clientName": "Jean Dupont",
  "status": "waiting",
  "createdAt": "2026-06-20T14:32:00Z"
}
```

| Champ                 | Type       | Description                                       |
| --------------------- | ---------- | ------------------------------------------------- |
| `id`                  | `Guid`     | Identifiant unique de l'entrée en file d'attente. |
| `businessQrCodeToken` | `Guid`     | QR token de l'entreprise (remplace `businessId`). |
| `businessName`        | `string`   | Nom de l'entreprise.                              |
| `position`            | `int`      | Position du client dans la file (commence à 1).   |
| `estimatedWaitTime`   | `int`      | Temps d'attente estimé en minutes.                |
| `phone`               | `string`   | Numéro de téléphone du client.                    |
| `clientName`          | `string`   | Nom du client (peut être `null` si non fourni).   |
| `status`              | `string`   | Toujours `"waiting"` à la création.               |
| `createdAt`           | `DateTime` | Horodatage UTC de l'inscription.                  |

---

##### ❌ `400 Bad Request` – File d'attente fermée

Retourné si `business.IsQueueActive == false`.

```
La file d'attente est fermée.
```

---

##### ❌ `400 Bad Request` – Numéro déjà en file

Retourné si le numéro `phone` est déjà présent avec le statut `"waiting"` pour cette entreprise.

```
Ce numéro est déjà dans la file d'attente.
```

---

##### ❌ `400 Bad Request` – File pleine

Retourné si le nombre de clients en attente est supérieur ou égal à `business.MaxQueueSize` (défaut : `50`).

```
La file d'attente est pleine.
```

---

##### ❌ `404 Not Found` – Entreprise introuvable

Retourné si aucune entreprise ne correspond au `qrCodeToken` fourni.

```
Entreprise non trouvée.
```

---

##### ❌ `500 Internal Server Error` – Erreur inattendue

Retourné pour toute exception non couverte par les cas ci-dessus.

```
Une erreur est survenue.
```

---

#### Exemple de requête

```http
POST /api/queue/join HTTP/1.1
Host: [À compléter]
Content-Type: application/json

{
  "qrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phone": "+33612345678",
  "clientName": "Jean Dupont"
}
```

---

#### Exemple de réponse (`200 OK`)

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "businessQrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "businessName": "Ma Boulangerie",
  "position": 3,
  "estimatedWaitTime": 10,
  "phone": "+33612345678",
  "clientName": "Jean Dupont",
  "status": "waiting",
  "createdAt": "2026-06-20T14:32:00Z"
}
```

---

#### Dépendances internes

| Composant                 | Rôle                                                                               |
| ------------------------- | ---------------------------------------------------------------------------------- |
| `BusinessService`         | Résolution de l'entreprise depuis le `qrCodeToken` (`FindBusinessByQrTokenAsync`). |
| `AppDbContext` (`Queues`) | Vérification de doublon, comptage des clients en attente, persistance de l'entrée. |
| `QueuePositionHelper`     | Calcul de la position d'insertion (`CalculateNewPosition`).                        |

---

#### Notes

- L'endpoint ne nécessite aucune authentification : il est conçu pour être appelé depuis un formulaire public accessible après scan du QR code.
- L'endpoint est soumis au rate limiter `"fixed"` (`[EnableRateLimiting("fixed")]`) configuré au niveau du contrôleur `QueueController`.
- La vérification du doublon porte uniquement sur les entrées avec `status == "waiting"` : un client ayant déjà été `"called"`, `"served"` ou `"cancelled"` peut se réinscrire avec le même numéro.
- Le temps d'attente estimé (`estimatedWaitTime`) est une valeur entière calculée par division entière ; pour une file vide (`waitingCount == 0`), la valeur retournée est `0`.

---

### `POST /api/business/generate:{businessQRCodeToken}/qrcode` – Générer un nouveau QR code pour une entreprise

#### Description

Permet au propriétaire d'une entreprise existante de générer un QR code pointant vers sa file d'attente. L'endpoint est réservé au gérant authentifié de l'entreprise identifiée par `{businessQRCodeToken}`. Le QR code encode l'URL `{WaitifyUrl}/q/{businessQRCodeToken}` et est retourné sous forme de balise HTML `<img>` embarquant une image PNG en base64.

---

#### Requête HTTP

- **Méthode :** `POST`
- **Chemin :** `/api/business/generate:{businessQRCodeToken}/qrcode`
- **Authentification requise :** Oui (type : Bearer JWT — extraction manuelle du claim `nameidentifier`)

> ⚠️ Aucun attribut `[Authorize]` n'est présent sur cet endpoint. L'authentification est vérifiée manuellement via `TokenService` : l'absence de token valide entraîne un `404` et non un `401`.

##### Headers obligatoires

| Header          | Valeur           |
| --------------- | ---------------- |
| `Authorization` | `Bearer <token>` |

##### Headers optionnels

_Aucun header optionnel identifié dans le code._

##### Paramètres de chemin (Path parameters)

| Paramètre             | Type   | Obligatoire | Description                        |
| --------------------- | ------ | ----------- | ---------------------------------- |
| `businessQRCodeToken` | `Guid` | ✅ Oui      | Identifiant de l'entreprise cible. |

> **Note :** La syntaxe du chemin est `generate:{businessQRCodeToken}/qrcode` (deux-points avant le paramètre). Exemple : `/api/business/generate:3fa85f64-5717-4562-b3fc-2c963f66afa6/qrcode`.

##### Paramètres de requête (Query string)

| Paramètre             | Type   | Obligatoire | Description                                                                                             |
| --------------------- | ------ | ----------- | ------------------------------------------------------------------------------------------------------- |
| `businessQRCodeToken` | `Guid` | ✅ Oui      | Token unique à encoder dans l'URL du QR code. Construit l'URL : `{WaitifyUrl}/q/{businessQRCodeToken}`. |

##### Body

Aucun body attendu.

---

#### Comportement serveur

1. Extraction de l'ID utilisateur depuis le claim JWT `nameidentifier` via `TokenService`.
2. Si le claim est absent ou invalide → `404 Not Found`.
3. Recherche de l'entreprise en base via `FindBusinessByIdAsync(id)`.
4. Si l'entreprise est introuvable → lève `KeyNotFoundException` (non interceptée dans le contrôleur).
5. Vérification que `userId == business.OwnerId` → si l'utilisateur n'est pas le propriétaire, lève `KeyNotFoundException`.
6. Construction de l'URL du QR code : `{AppConstants.Config.WaitifyUrl}/q/{businessQRCodeToken}`.
7. Génération du QR code via `QRCodeGeneratorService.GenerateQRCode(url)` :
   - Niveau de correction d'erreur : `ECCLevel.Q`.
   - Format de sortie : image PNG encodée en base64, encapsulée dans une balise `<img>`.
8. Retour du QR code généré (`200 OK`).

---

#### Réponses

##### ✅ `200 OK` – Succès

Retourne une chaîne HTML contenant le QR code sous forme d'image PNG en base64.

```html
<img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA..." />
```

| Champ  | Type     | Description                                                          |
| ------ | -------- | -------------------------------------------------------------------- |
| (body) | `string` | Balise `<img>` avec l'image PNG du QR code encodée en base64 inline. |

---

##### ❌ `404 Not Found` – Claim JWT absent ou invalide

Retourné si `TokenService` ne peut pas extraire l'ID utilisateur depuis le token JWT.

```
Utilisateur introuvable ou accès refusé.
```

---

##### ❌ `404 Not Found` – QR code non généré

Retourné si `GenerateNewQRCodeAsync` retourne `null`.

```
QRCode non généré.
```

---

##### ❌ `500 Internal Server Error` – Entreprise ou utilisateur introuvable / accès refusé

Les cas suivants lèvent une `KeyNotFoundException` non interceptée dans le contrôleur :

| Condition                                               | Message interne                                   |
| ------------------------------------------------------- | ------------------------------------------------- |
| Entreprise introuvable (`FindBusinessByIdAsync` = null) | `"Entreprise non trouvée."`                       |
| Utilisateur introuvable ou condition de vérification    | `"Utilisateur non trouvé ou accès non autorisé."` |
| `userId != business.OwnerId`                            | `"Utilisateur non trouvé."`                       |

> Le code HTTP résultant dépend du gestionnaire global d'erreurs de l'application — [À compléter].

---

#### Exemple de requête

```http
POST /api/business/generate:3fa85f64-5717-4562-b3fc-2c963f66afa6/qrcode?qrCodeToken=a1b2c3d4-e5f6-7890-abcd-ef1234567890 HTTP/1.1
Host: [À compléter]
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

#### Exemple de réponse (`200 OK`)

```html
<img
  src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASwAAAEsCAYAAAB5fY51AAAA..."
/>
```

---

#### Dépendances internes

| Composant                        | Rôle                                                                               |
| -------------------------------- | ---------------------------------------------------------------------------------- |
| `TokenService`                   | Extraction du claim `nameidentifier` depuis le JWT.                                |
| `BusinessService`                | Recherche de l'entreprise (`FindBusinessByIdAsync`), vérification du propriétaire. |
| `ApplicationUserService`         | Vérification de l'existence de l'utilisateur (`FindUserByIdAsync`).                |
| `QRCodeGeneratorService`         | Génération du QR code PNG en base64 (`GenerateQRCode`).                            |
| `AppConstants.Config.WaitifyUrl` | URL de base utilisée pour construire le lien encodé dans le QR code.               |

---

#### Variables d'environnement requises

| Variable      | Usage                                                   |
| ------------- | ------------------------------------------------------- |
| [À compléter] | `AppConstants.Config.WaitifyUrl` – URL de base Waitify. |

---

#### Notes

- Le paramètre `qrCodeToken` est fourni par le client dans la query string. Il n'est pas généré côté serveur dans cet endpoint (contrairement à `POST /api/business` où il est généré automatiquement). Le client est responsable de passer un token cohérent avec le `QrCodeToken` stocké en base pour l'entreprise.
- L'endpoint est soumis au rate limiter `"fixed"` configuré au niveau du contrôleur `BusinessController`.
- La vérification de l'utilisateur dans `GenerateNewQRCodeAsync` présente une anomalie : `FindUserByIdAsync` est appelé sans `await`, ce qui signifie que la variable `existingUser` est en réalité une `Task<ApplicationUser>` et non un `ApplicationUser`. La condition `existingUser?.Id.ToString() == userId` compare l'ID de la tâche (entier) à un GUID, et sera toujours `false`. La vérification effective du propriétaire repose donc uniquement sur la comparaison `userId != business.OwnerId`.
- Le QR code généré encode l'URL `{WaitifyUrl}/q/{qrCodeToken}` avec un niveau de correction d'erreur `ECCLevel.Q` (≈ 25 % de capacité de correction).

---

### `GET /api/queue/{id}` – Récupérer une entrée de file d'attente

#### Description

Récupère les détails d'une entrée de file d'attente par son identifiant unique. Endpoint destiné aux usages internes (tableau de bord gérant, suivi client).

---

#### Requête HTTP

- **Méthode :** `GET`
- **Chemin :** `/api/queue/{id}`
- **Authentification requise :** Non

##### Paramètres de chemin (Path parameters)

| Paramètre | Type   | Obligatoire | Description                                       |
| --------- | ------ | ----------- | ------------------------------------------------- |
| `id`      | `Guid` | ✅ Oui      | Identifiant unique de l'entrée en file d'attente. |

---

#### Comportement serveur

1. Recherche de l'entrée via `FindQueueByIdAsync(id)`.
2. Si introuvable → `404 Not Found`.
3. Retour de l'entité `QueueEntries` complète.

---

#### Réponses

##### ✅ `200 OK` – Succès

Retourne l'entité `QueueEntries` complète (tous les champs de l'entité).

##### ❌ `404 Not Found` – Entrée introuvable

```
File d'attente introuvable
```

---

### `POST /api/queue/{qrCodeToken}/call-next` – Appeler le prochain client

#### Description

Appelle le prochain client en attente dans la file de l'entreprise identifiée par son `qrCodeToken`. Passe le statut du client de `"waiting"` à `"called"` et recalcule les positions des clients restants.

> **Note de sécurité :** L'entreprise est identifiée par son `QrCodeToken` (et non par son `Id` interne), conformément à la politique de sécurité de l'API.

---

#### Requête HTTP

- **Méthode :** `POST`
- **Chemin :** `/api/queue/{qrCodeToken}/call-next`
- **Authentification requise :** Non (à sécuriser côté front-end)

##### Paramètres de chemin (Path parameters)

| Paramètre     | Type   | Obligatoire | Description                                |
| ------------- | ------ | ----------- | ------------------------------------------ |
| `qrCodeToken` | `Guid` | ✅ Oui      | QR token unique de l'entreprise concernée. |

##### Body

Aucun body attendu.

---

#### Comportement serveur

1. Recherche de l'entreprise via `FindBusinessByQrTokenAsync(qrCodeToken)`.
2. Si introuvable → `404 Not Found`.
3. Si `business.IsQueueActive == false` → `400 Bad Request`.
4. Récupération du client ayant la position la plus basse avec le statut `"waiting"`.
5. Si aucun client en attente → `400 Bad Request`.
6. Passage du statut à `"called"`, mise à jour de `CalledAt` et `UpdatedAt`.
7. Recalcul des positions des clients restants en `"waiting"` (`QueuePositionHelper.RecalculatePositionsAsync`).
8. Persistance et retour de `CallNextClientResponse`.

---

#### Réponses

##### ✅ `200 OK` – Succès

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "businessQrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phone": "+33612345678",
  "clientName": "Jean Dupont",
  "position": 1,
  "status": "called",
  "calledAt": "2026-06-20T14:45:00Z"
}
```

| Champ                 | Type       | Description                                |
| --------------------- | ---------- | ------------------------------------------ |
| `id`                  | `Guid`     | Identifiant de l'entrée en file d'attente. |
| `businessQrCodeToken` | `Guid`     | QR token de l'entreprise.                  |
| `phone`               | `string`   | Numéro de téléphone du client.             |
| `clientName`          | `string`   | Nom du client (peut être `null`).          |
| `position`            | `int`      | Position au moment de l'appel.             |
| `status`              | `string`   | Toujours `"called"` après succès.          |
| `calledAt`            | `DateTime` | Horodatage UTC de l'appel.                 |

##### ❌ `400 Bad Request` – File fermée

```
La file d'attente est fermée.
```

##### ❌ `400 Bad Request` – Aucun client en attente

```
Aucun client en attente dans la file.
```

##### ❌ `404 Not Found` – Entreprise introuvable

```
Entreprise non trouvée.
```

##### ❌ `500 Internal Server Error` – Erreur inattendue

```
Une erreur est survenue.
```

---

#### Dépendances internes

| Composant             | Rôle                                                              |
| --------------------- | ----------------------------------------------------------------- |
| `BusinessService`     | Résolution de l'entreprise via `FindBusinessByQrTokenAsync`.      |
| `AppDbContext`        | Requête sur la file, mise à jour du statut, persistance.          |
| `QueuePositionHelper` | Recalcul des positions après appel (`RecalculatePositionsAsync`). |

---

### `DELETE /api/queue/{id}/cancel` – Annuler une entrée de file d'attente

#### Description

Annule l'entrée d'un client dans la file d'attente. Seules les entrées avec le statut `"waiting"` peuvent être annulées. Les positions des clients restants sont recalculées après annulation.

---

#### Requête HTTP

- **Méthode :** `DELETE`
- **Chemin :** `/api/queue/{id}/cancel`
- **Authentification requise :** Non

##### Paramètres de chemin (Path parameters)

| Paramètre | Type   | Obligatoire | Description                                       |
| --------- | ------ | ----------- | ------------------------------------------------- |
| `id`      | `Guid` | ✅ Oui      | Identifiant unique de l'entrée en file d'attente. |

##### Body

Aucun body attendu.

---

#### Comportement serveur

1. Recherche de l'entrée via `FindAsync(id)`.
2. Si introuvable → `404 Not Found`.
3. Si `entry.Status != "waiting"` → `400 Bad Request`.
4. Passage du statut à `"cancelled"`, mise à jour de `UpdatedAt`.
5. Recalcul des positions des clients restants en `"waiting"`.
6. Persistance et retour de `CancelQueueEntryResponse`.

---

#### Réponses

##### ✅ `200 OK` – Succès

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "businessQrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phone": "+33612345678",
  "clientName": "Jean Dupont",
  "status": "cancelled",
  "updatedAt": "2026-06-20T14:50:00Z"
}
```

| Champ                 | Type       | Description                          |
| --------------------- | ---------- | ------------------------------------ |
| `id`                  | `Guid`     | Identifiant de l'entrée.             |
| `businessQrCodeToken` | `Guid`     | QR token de l'entreprise.            |
| `phone`               | `string`   | Numéro de téléphone du client.       |
| `clientName`          | `string`   | Nom du client (peut être `null`).    |
| `status`              | `string`   | Toujours `"cancelled"` après succès. |
| `updatedAt`           | `DateTime` | Horodatage UTC de l'annulation.      |

##### ❌ `400 Bad Request` – Statut incompatible

Retourné si l'entrée n'est pas en statut `"waiting"`.

```
Impossible d'annuler une entrée avec le statut '<statut>'.
```

##### ❌ `404 Not Found` – Entrée introuvable

```
Entrée de file d'attente introuvable.
```

##### ❌ `500 Internal Server Error` – Erreur inattendue

```
Une erreur est survenue.
```

---

#### Notes

- La vérification du statut ne porte que sur `"waiting"` : un client `"called"` ou `"served"` ne peut pas être annulé via cet endpoint.

---

### `PATCH /api/queue/{id}/served` – Marquer un client comme servi

#### Description

Marque un client comme servi. Seules les entrées avec le statut `"called"` peuvent être marquées comme servies. Permet d'enregistrer optionnellement le temps de service réel.

---

#### Requête HTTP

- **Méthode :** `PATCH`
- **Chemin :** `/api/queue/{id}/served`
- **Authentification requise :** Non

##### Headers obligatoires

| Header         | Valeur             |
| -------------- | ------------------ |
| `Content-Type` | `application/json` |

##### Paramètres de chemin (Path parameters)

| Paramètre | Type   | Obligatoire | Description                                       |
| --------- | ------ | ----------- | ------------------------------------------------- |
| `id`      | `Guid` | ✅ Oui      | Identifiant unique de l'entrée en file d'attente. |

##### Body (`application/json`)

| Champ               | Type  | Obligatoire | Description                                            |
| ------------------- | ----- | ----------- | ------------------------------------------------------ |
| `actualServiceTime` | `int` | ❌ Non      | Durée réelle du service en secondes. Ignoré si `null`. |

```json
{
  "actualServiceTime": 240
}
```

---

#### Comportement serveur

1. Recherche de l'entrée via `FindAsync(id)`.
2. Si introuvable → `404 Not Found`.
3. Si `entry.Status != "called"` → `400 Bad Request`.
4. Passage du statut à `"served"`, mise à jour de `ServedAt` et `UpdatedAt`.
5. Si `actualServiceTime` est fourni → stockage dans `entry.ActualServiceTime`.
6. Persistance et retour de `MarkClientAsServedResponse`.

---

#### Réponses

##### ✅ `200 OK` – Succès

```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "businessQrCodeToken": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "phone": "+33612345678",
  "clientName": "Jean Dupont",
  "status": "served",
  "calledAt": "2026-06-20T14:45:00Z",
  "servedAt": "2026-06-20T14:49:00Z",
  "actualServiceTime": 240
}
```

| Champ                 | Type       | Description                                                  |
| --------------------- | ---------- | ------------------------------------------------------------ |
| `id`                  | `Guid`     | Identifiant de l'entrée.                                     |
| `businessQrCodeToken` | `Guid`     | QR token de l'entreprise.                                    |
| `phone`               | `string`   | Numéro de téléphone du client.                               |
| `clientName`          | `string`   | Nom du client (peut être `null`).                            |
| `status`              | `string`   | Toujours `"served"` après succès.                            |
| `calledAt`            | `DateTime` | Horodatage UTC de l'appel du client.                         |
| `servedAt`            | `DateTime` | Horodatage UTC de la fin du service.                         |
| `actualServiceTime`   | `int?`     | Temps de service réel en secondes (`null` si non renseigné). |

##### ❌ `400 Bad Request` – Statut incompatible

Retourné si l'entrée n'est pas en statut `"called"`.

```
Impossible de marquer comme servi une entrée avec le statut '<statut>'.
```

##### ❌ `404 Not Found` – Entrée introuvable

```
Entrée de file d'attente introuvable.
```

##### ❌ `500 Internal Server Error` – Erreur inattendue

```
Une erreur est survenue.
```

---

#### Notes

- `actualServiceTime` retourné est `null` si la valeur stockée est `0` (valeur par défaut EF Core).
- Aucun recalcul de positions n'est effectué : les clients en `"called"` ne font plus partie de la file `"waiting"`.
