using WaitifyApi.Models;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Entities;
using WaitifyApi.Repositories;
using WaitifyApi.Enums;
using Newtonsoft.Json;
using System.Security.Claims;
using WaitifyApi.Exceptions;

namespace WaitifyApi.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    IAuthTokenProcessor authTokenProcessor,
    FileStorageService fileStorageService,
    IEmailRepository emailService,
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
        string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobUsersContainer")!;

        if (request.ProfilePicture is not null)
        {
            string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
            profilePictureUrl = await fileStorageService.UploadBlobAsync(request.ProfilePicture, userName, azureContainerName, allowedExtensions);
        }

        TimeSpan TwoWeeks = new TimeSpan(360, 0, 0);
        DateTime trialEndDate = DateTime.UtcNow.Add(TwoWeeks);


        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.LastName + request.FirstName,
            Email = request.Email,
            Role = Role.Owner,
            TrialEndsAt = trialEndDate,
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
        logger.LogInformation("Token de connexion : {@0}", JsonConvert.SerializeObject(token, Formatting.Indented));

        var Email = request?.Email;
        var UserName = $"{request?.FirstName} {request?.LastName}";
        var createdAt = DateTime.UtcNow.ToString();
        string url = "https://waitify.fr/unsusbcribe";
        string UserId = user.Id;
        var TrialEndDate = trialEndDate.ToString();

        await emailService.RegisterEmail(Email, UserName, createdAt, url);
        await emailService.NewUserAcquiredEmail(Email, UserName, UserId, createdAt, TrialEndDate);

        logger.LogInformation("Emails envoyé avec succès.");

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

    public async Task<string> LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
        {
            throw new ExternalLoginProviderException("Google", "ClaimsPrincipal est `null`");
        }

        var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

        if (email == null)
        {
            throw new ExternalLoginProviderException("Google", "Email est `null`");
        }

        var user = await userManager.FindByEmailAsync(email);
        var GoogleApiKey = Environment.GetEnvironmentVariable("GoogleApiKey");
        var GoogleProfilePicture = $"https://people.googleapis.com/v1/people/{claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)}?personFields=photos&key=image&key={GoogleApiKey}";
        // using (HttpClient httpClient = new HttpClient())
        // {
        //     string s = await httpClient.GetStringAsync(GoogleProfilePicture);
        //     dynamic deserializeObject = JsonConvert.DeserializeObject(s);
        //     string thumbnailUrl = (string)deserializeObject.image.url;
        //     byte[] thumbnail = await httpClient.GetByteArrayAsync(thumbnailUrl);
        // }
        // var GoogleProfilePictureUrl = GoogleProfilePicture[0].url;

        TimeSpan TwoWeeks = new TimeSpan(360, 0, 0);
        DateTime trialEndDate = DateTime.UtcNow.Add(TwoWeeks);

        if (user == null)
        {
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                EmailConfirmed = true,
                AuthProvider = "Google",
                GoogleId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                Role = Role.Owner,
                PhoneNumber = claimsPrincipal.FindFirstValue(ClaimTypes.HomePhone) ?? claimsPrincipal.FindFirstValue(ClaimTypes.MobilePhone),
                ProfilePicture = GoogleProfilePicture,
                TrialEndsAt = trialEndDate,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
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

        var existingLogins = await userManager.GetLoginsAsync(user);
        if (!existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
        {
            var loginResult = await userManager.AddLoginAsync(user, info);

            if (!loginResult.Succeeded)
            {
                throw new ExternalLoginProviderException("Google",
                    $"Unable to login user: {string.Join(", ", loginResult.Errors.Select(x => x.Description))}");
            }
        }


        var (jwtToken, expirationDateInUtc) = authTokenProcessor.GenerateJwtToken(user);
        var refreshTokenValue = authTokenProcessor.GenerateRefreshToken();

        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiryTime = refreshTokenExpirationDateInUtc;

        await userManager.UpdateAsync(user);

        logger.LogInformation("Google JWT Token : {@0}", jwtToken);

        authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("waitify_token", jwtToken, expirationDateInUtc);
        authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);

        return jwtToken;
    }
}
