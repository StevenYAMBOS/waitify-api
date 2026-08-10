using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class AuthTokenProcessor : IAuthTokenProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(ApplicationUser user)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("AppSettingsToken")));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var expires = DateTime.UtcNow.AddMinutes(10.5);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("AppSettingsIssuer"),
            audience: Environment.GetEnvironmentVariable("AppSettingsAudience"),
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expires);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token,
        DateTime expiration)
    {
        _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName,
            token, new CookieOptions
            {
                HttpOnly = true,
                Expires = expiration,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
    }
}