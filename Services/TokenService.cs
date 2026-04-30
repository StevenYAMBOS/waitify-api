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

    public async Task<string?> GetJwtTokenFromRequest(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }
        if (!authHeader.StartsWith("Bearer "))
        {
            return authHeader;
        }
        return authHeader.Substring("Bearer ".Length).Trim();

    }

    public async Task<string?> GetInformationFromToken(HttpContext context, string dataProp)
    {
        var token = await GetJwtTokenFromRequest(context);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenS = tokenHandler.ReadToken(token) as JwtSecurityToken;
            var targetInfo = tokenS!.Claims.FirstOrDefault(claim => claim.Type == dataProp);
            if (targetInfo != null)
            {
                return targetInfo.Value;
            }
            return null;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    /*     public async Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ExternalLoginProviderException("Google", "ClaimsPrincipal is null");
            }

            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                throw new ExternalLoginProviderException("Google", "Email is null");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                    LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser);

                if (!result.Succeeded)
                {
                    throw new ExternalLoginProviderException("Google",
                        $"Unable to create user: {string.Join(", ",
                            result.Errors.Select(x => x.Description))}");
                }



                user = newUser;
            }

            var info = new UserLoginInfo("Google",
                claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                "Google");

            var loginResult = await userManager.AddLoginAsync(user, info);

            if (!loginResult.Succeeded)
            {
                throw new ExternalLoginProviderException("Google",
                    $"Unable to login user: {string.Join(", ",
                        loginResult.Errors.Select(x => x.Description))}");
            }

            var jwtToken = CreateTokenAsync(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshTokenValue;
            // user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

            await userManager.UpdateAsync(user);

            // tokenService.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
            // tokenService.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
        } */
}