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
