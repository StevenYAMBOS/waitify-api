## JWT invalide — `WWW-Authenticate: Bearer error="invalid_token"`

**Symptôme :** Le token JWT est bien décodé sur jwt.io mais ASP.NET Core retourne `invalid_token` sur les endpoints `[Authorize]`.

### Causes et corrections

**Package manquant**

Le package `Microsoft.IdentityModel.JsonWebTokens` est requis pour la validation JWT avec `Microsoft.AspNetCore.Authentication.JwtBearer`.

```bash
dotnet add package Microsoft.IdentityModel.JsonWebTokens
```

**`RequireHttpsMetadata = true` en développement**

Si l'API tourne en HTTP (ex. `http://localhost:5258`), le middleware rejette le token car HTTPS est exigé.

```csharp
// Avant
options.RequireHttpsMetadata = true;

// Après
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
```

**Mauvais ordre des middlewares**

`UseCors()` doit être placé **avant** `UseAuthentication()`.

```csharp
// Correct
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
```

**Debugging — logs JWT**

Ajouter `OnAuthenticationFailed` et `OnChallenge` dans `JwtBearerEvents` pour voir la cause exacte dans la console :

```csharp
OnAuthenticationFailed = ctx =>
{
    Console.WriteLine($"Auth failed: {ctx.Exception.GetType().Name} - {ctx.Exception.Message}");
    return Task.CompletedTask;
},
OnChallenge = ctx =>
{
    Console.WriteLine($"Challenge: Error={ctx.Error}, Description={ctx.ErrorDescription}");
    return Task.CompletedTask;
}
```

### Liens utiles

Liens sombres mais très utiles.

- [CodeGenes.net - Blog](https://www.codegenes.net/blog/c-asp-net-core-bearer-error-invalid-token/) la réponse validé est un plus.
- [Stack Overflow - How to debug Bearer error="invalid_token"](https://stackoverflow.com/questions/75336709/how-to-debug-bearer-error-invalid-token) pour les tests `curl`.
- [Stack Overflow - JWTBearer error on calling authorized method](https://stackoverflow.com/questions/61247965/jwtbearer-error-on-calling-authorized-method)
- [Forum Microsoft](https://learn.microsoft.com/en-us/answers/questions/5646974/swagger-addsecurityrequirement-fails-after-migrati)
- [Stack Overflow - Authentication failed: method not found](https://stackoverflow.com/questions/79440613/authentication-failed-method-not-found) réponse de Andrew Shepherd le GOAT.

## Génération d'un QRCode

Ce n'est pas comme dans les autres langages, la réponse est en format brut (`json`, `raw`, `Base64`).
Mieux vaut utiliser la librairie [QRCoder](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#23-base64qrcode-renderer-in-detail), par contre certaines fonctions ne fonctionnent QUE SUR WINDOWS ⚠️ !

- [Stack Overflow - Réponse de riffnl le GOAT](https://stackoverflow.com/a/78542724/17386694)

Exemple d'implémentation :

```csharp
QRCodeGenerator qrGenerator = new QRCodeGenerator();
var qrString = $"URL ou autre";
QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(5);
string base64String = Convert.ToBase64String(qrCodeAsPngByteArr, 0, qrCodeAsPngByteArr.Length);

var base64Png = $"<img src='data:image/png;base64,{base64String}' />";
```

---

## Authentification Google OAuth2

Il ne semble pas y avoir de solution "générale". En tout cas, le endpoint par défaut a intégrer sur GCP est `/signin-google` -> `https://localhost:{port}/signin-google`.

J'ai d'abord testé la solution officielle proposée par [Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-9.0&tabs=visual-studio). Cependant elle ne propose pas d'intégration avec des controlleurs ni de démonstration avec l'URL à tester (exemple : `https://localhost:{port}/signin-google` ne fonctionne pas, donc c'est quoi l'URL a testé ?).

La solution que j'ai intégré est [celle-ci](https://github.com/RemigiuszZalewski/Google-Authentication-.net-react).

### Récupérer profil picture utilisateur

Liens utils :

- https://stackoverflow.com/questions/62703154/google-authentication-get-user-profile-picture
- https://stackoverflow.com/questions/13297563/read-and-parse-a-json-file-in-c-sharp

---

## Email template avec paramètres

Meilleure documentation ici -> https://www.aspsnippets.com/Articles/4250/Send-Email-with-HTML-Templates-using-MailKit-in-ASPNet-Core/.

Pour faire simple, on crée une méthode privée (enfant) qui va stocker les paramètres et l'envoie de l'email puis on appelle cette méthode dans la méthode parente pour envoyer l'email avec le serveur SMTP.

Autres liens utiles (concernant l'appel des chemins des templates) :

- [Stack Overflow (réponse acceptée)](https://stackoverflow.com/questions/42478814/send-email-using-html-template-with-mailkit-in-net-core-application).
- [Stack Overflow (réponse de Cinchoo)](https://stackoverflow.com/questions/10623656/streamreader-to-a-relative-filepath).

---

## Setup des tests

Solution : https://stackoverflow.com/a/62426421/17386694

Le projet de test ne doit pas être dans le projet de base, chaque projets doivent être dans un dossier différent (sinon il y a "collision").

```
MyProject
   src/MyProject.csproj
   tests/MyTestProject.csproj
```
