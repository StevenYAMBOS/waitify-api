using WaitifyApi.Models;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Entities;
using WaitifyApi.Repositories;
using WaitifyApi.Enums;
using Newtonsoft.Json;

namespace WaitifyApi.Services;

public class AuthService(UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    FileStorageService fileStorageService,
    ILogger<AuthService> logger) : IAuthRepository
{
    public async Task<(bool Success, string? Token, IEnumerable<string>? Errors)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email!);
        if (existingUser is not null)
        {
            logger.LogWarning("Inscription échouée : {Email} déjà utilisé", request.Email);
            return (false, null, ["Cette adresse email est déjà utilisée."]);
        }

        string? profilePictureUrl = null;
        string userName = $"{request.FirstName + request.LastName}";

        if (request.ProfilePicture is not null)
        {
            string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
            profilePictureUrl = await fileStorageService.UploadBlobAsync(request.ProfilePicture, userName, allowedExtensions);
        }


        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.FirstName + request.LastName,
            Email = request.Email,
            Role = Role.Owner,
            ProfilePicture = profilePictureUrl,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            logger.LogWarning("Inscription échouée pour {Email} : {Errors}", request.Email, string.Join(", ", errors));
            return (false, null, errors);
        }

        await userManager.AddToRoleAsync(user, Role.Owner.ToString());

        var token = await tokenService.CreateTokenAsync(user);
        logger.LogInformation("Token de connexion : {0}", JsonConvert.SerializeObject(token, Formatting.Indented));

        return (true, token, null);
    }

    public async Task<(bool Success, TokenResponseDTO? Tokens, string? Error)> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);
        if (user is null)
        {
            logger.LogWarning("Connexion échouée : utilisateur {Email} introuvable", request.Email);
            return (false, null, "Identifiants incorrects.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password!);
        if (!passwordValid)
        {
            logger.LogWarning("Connexion échouée : mot de passe incorrect pour {Email}", request.Email);
            return (false, null, "Identifiants incorrects.");
        }

        var tokens = await GenerateTokensAsync(user);

        return (true, tokens, null);
    }

    private async Task<TokenResponseDTO> GenerateTokensAsync(ApplicationUser user)
    {
        user.LastLogin = DateTime.UtcNow;
        user.RefreshToken = tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return new TokenResponseDTO
        {
            AccessToken = await tokenService.CreateTokenAsync(user),
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<(bool Success, TokenResponseDTO? Tokens, string? Error)> RefreshTokensAsync(RefreshTokenRequestDTO request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null || user.RefreshToken != request.RefreshToken)
        {
            logger.LogWarning("Refresh token invalide pour UserId {UserId}", request.UserId);
            return (false, null, "Refresh token invalide.");
        }

        if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token expiré pour UserId {UserId}", request.UserId);
            return (false, null, "Refresh token expiré, veuillez vous reconnecter.");
        }

        var tokens = await GenerateTokensAsync(user);
        return (true, tokens, null);
    }
}