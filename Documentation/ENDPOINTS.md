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
