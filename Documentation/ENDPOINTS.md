# Documentation des routes API

Modifié le : 01/06/2026

Par : [Steven YAMBOS](www.linkedin.com/in/steven-yambos)

## Routes

### Routes entreprises

### Créer une entreprise

>Chemin : `POST /api/business`

#### Description

Cet endpoint permet à un utilisateur authentifié de créer une nouvelle entreprise associée à son compte. Il est destiné aux utilisateurs disposant d'un token JWT valide. La création génère automatiquement un QR code unique pointant vers la file d'attente de l'entreprise.

---

#### Requête HTTP

- **Méthode :** `POST`
- **Chemin :** `/api/business`
- **Authentification requise :** Oui (type : Bearer JWT)

##### Headers obligatoires

| Header          | Valeur                        |
|-----------------|-------------------------------|
| `Authorization` | `Bearer <token>`              |
| `Content-Type`  | `multipart/form-data`         |

> ⚠️ Le body est envoyé en `multipart/form-data` (et non `application/json`) car il peut contenir un fichier image (`Logo`). Tout client HTTP doit utiliser ce content-type.

##### Headers optionnels

_Aucun header optionnel identifié dans le code._

---

#### Body (`multipart/form-data`)

| Champ          | Type         | Obligatoire | Contraintes                                                                  | Description                          |
|----------------|--------------|-------------|------------------------------------------------------------------------------|--------------------------------------|
| `Name`         | `string`     | ✅ Oui      | —                                                                            | Nom de l'entreprise.                 |
| `BusinessType` | `string`     | ✅ Oui      | —                                                                            | Type / catégorie de l'entreprise.    |
| `PhoneNumber`  | `string`     | ✅ Oui      | —                                                                            | Numéro de téléphone de l'entreprise. |
| `Address`      | `string`     | ✅ Oui      | —                                                                            | Adresse postale.                     |
| `City`         | `string`     | ✅ Oui      | —                                                                            | Ville.                               |
| `ZipCode`      | `string`     | ✅ Oui      | —                                                                            | Code postal.                         |
| `Country`      | `string`     | ✅ Oui      | Valeur par défaut : `"France"`                                               | Pays.                                |
| `Logo`         | `IFormFile`  | ❌ Non      | Taille max : **1 Mo**. Extensions autorisées : `.jpeg`, `.jpg`, `.png`, `.webp`, `.svg` | Logo de l'entreprise.   |

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

| Champ          | Message d'erreur                                        |
|----------------|---------------------------------------------------------|
| `Name`         | `"Le nom est incorrecte."`                              |
| `BusinessType` | `"Le type est obligatoire."`                            |
| `PhoneNumber`  | `"Le format du numéro de téléphone est incorrecte."`    |
| `Address`      | `"L'adresse est incorrecte."`                           |
| `City`         | `"La ville est obligatoire."`                           |
| `ZipCode`      | `"Le code postale est obligatoire."`                    |
| `Country`      | `"Le pays est obligatoire."`                            |

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

| Composant                  | Rôle                                                            |
|----------------------------|-----------------------------------------------------------------|
| `TokenService`             | Extraction du claim `nameidentifier` depuis le JWT.            |
| `ApplicationUserService`   | Vérification de l'existence de l'utilisateur (`FindUserByIdAsync`). |
| `FileStorageService`       | Upload du logo vers Azure Blob Storage (`UploadBlobAsync`).    |
| `QRCodeHelper`             | Génération du QR code (`GenerateQRCode`).                      |
| `AppDbContext`             | Persistance de l'entité `Business`.                            |
| `AppConstants.WaitifyUrl`  | URL de base utilisée pour construire le lien du QR code.       |

---

### Variables d'environnement requises

| Variable                        | Usage                                              |
|---------------------------------|----------------------------------------------------|
| `AzureBlobBusinessesContainer`  | Nom du conteneur Azure Blob pour les logos.        |
| [À compléter]                   | `AppConstants.WaitifyUrl` – URL de base Waitify.   |

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

| Paramètre   | Type     | Obligatoire | Description                                                                                     |
|-------------|----------|-------------|--------------------------------------------------------------------------------------------------|
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

| Composant                            | Rôle                                                                              |
|--------------------------------------|-----------------------------------------------------------------------------------|
| `SignInManager<ApplicationUser>`     | Construction des propriétés d'authentification externe (`ConfigureExternalAuthenticationProperties`). |
| `LinkGenerator`                      | Résolution de l'URL de callback à partir du nom d'endpoint `GoogleLoginCallback`. |

---

#### Variables d'environnement requises

| Variable                        | Usage                                      |
|---------------------------------|--------------------------------------------|
| `AuhtenticationGoogleClientId`  | Client ID OAuth Google.                    |
| `AuhtenticationGoogleSecret`    | Client Secret OAuth Google.                |

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

| Paramètre   | Type     | Obligatoire | Description                                                                     |
|-------------|----------|-------------|---------------------------------------------------------------------------------|
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

      | Champ            | Source                                                                                     | Valeur par défaut     |
      |------------------|--------------------------------------------------------------------------------------------|-----------------------|
      | `UserName`       | `ClaimTypes.Email`                                                                         | —                     |
      | `Email`          | `ClaimTypes.Email`                                                                         | —                     |
      | `FirstName`      | `ClaimTypes.GivenName`                                                                     | `""` (vide)           |
      | `LastName`       | `ClaimTypes.Surname`                                                                       | `""` (vide)           |
      | `EmailConfirmed` | Fixé à `true`                                                                              | —                     |
      | `AuthProvider`   | Fixé à `"Google"`                                                                          | —                     |
      | `GoogleId`       | `ClaimTypes.NameIdentifier`                                                                | `""` (vide)           |
      | `Role`           | Fixé à `Role.Owner`                                                                        | —                     |
      | `PhoneNumber`    | `ClaimTypes.HomePhone` ou `ClaimTypes.MobilePhone`                                         | `null`                |
      | `ProfilePicture` | URL construite : `https://people.googleapis.com/v1/people/{NameIdentifier}?personFields=photos&key=image&key={GoogleApiKey}` | — |
      | `TrialEndsAt`    | `DateTime.UtcNow + 360h (15 jours)`                                                        | —                     |
      | `CreatedAt`      | `DateTime.UtcNow`                                                                          | —                     |
      | `LastLogin`      | `DateTime.UtcNow`                                                                          | —                     |

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

| Condition                                          | Message d'erreur interne                                    |
|----------------------------------------------------|--------------------------------------------------------------|
| `ClaimsPrincipal` est `null`                       | `"ClaimsPrincipal est null"`                                |
| Claim `Email` absent du principal                  | `"Email est null"`                                          |
| Échec de `userManager.CreateAsync`                 | `"Unable to create user: <détails Identity>"`               |
| Échec de `userManager.AddLoginAsync`               | `"Unable to login user: <détails Identity>"`                |

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

| Composant                        | Rôle                                                                                   |
|----------------------------------|----------------------------------------------------------------------------------------|
| `SignInManager<ApplicationUser>` | Authentification via le schéma Cookie (`AuthenticateAsync`).                          |
| `AuthService.LoginWithGoogleAsync` | Création/récupération de l'utilisateur et enregistrement du login externe Google.   |
| `UserManager<ApplicationUser>`   | Recherche, création et gestion des logins externes de l'utilisateur.                  |

---

#### Variables d'environnement requises

| Variable               | Usage                                                                  |
|------------------------|------------------------------------------------------------------------|
| `GoogleApiKey`         | Clé API Google utilisée pour construire l'URL de photo de profil.     |
| `AuhtenticationGoogleClientId` | Client ID OAuth Google (configuré au démarrage).             |
| `AuhtenticationGoogleSecret`   | Client Secret OAuth Google (configuré au démarrage).         |

---

#### Notes

- Cet endpoint est nommé `GoogleLoginCallback` via `[EndpointName("GoogleLoginCallback")]`, ce qui permet à `LinkGenerator` de résoudre son URL depuis `GoogleLogin()`.
- La photo de profil stockée dans `ProfilePicture` est une URL d'API Google People (non une image directe) et nécessite une clé API valide pour être utilisée.
- L'émission de token JWT post-authentification Google est actuellement désactivée (code commenté). L'intégration front-end ne peut pas récupérer de JWT à l'issue de ce flux en l'état.
- Aucun email de bienvenue n'est envoyé lors d'une inscription via Google (contrairement au flux `POST /api/auth/register`).
