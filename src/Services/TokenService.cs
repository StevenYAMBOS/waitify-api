using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Exceptions;

namespace WaitifyApi.Services;

public class TokenService(ILogger<TokenService> logger, UserManager<ApplicationUser> userManager)
{
    private const int ExpirationMinutes = 60;

    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var claims = await BuildClaimsAsync(user);
        var credentials = CreateSigningCredentials();
        var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("AppSettingsIssuer"),
            audience: Environment.GetEnvironmentVariable("AppSettingsAudience"),
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }

    private async Task<List<Claim>> BuildClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identityRoles = await userManager.GetRolesAsync(user);
        foreach (var role in identityRoles)
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        return claims;
    }

    private SigningCredentials CreateSigningCredentials()
    {
        var key = new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("AppSettingsToken"))
    );
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Lit un claim de l'utilisateur authentifié. L'identité provient du JWT déjà validé
    /// (signature, émetteur, audience, expiration) par le middleware d'authentification :
    /// un token non signé ou expiré renvoie <c>null</c>.
    /// </summary>
    public Task<string?> GetInformationFromToken(HttpContext context, string dataProp)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<string?>(null);
        }
        return Task.FromResult(context.User.FindFirst(dataProp)?.Value);
    }
}