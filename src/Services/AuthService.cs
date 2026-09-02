using WaitifyApi.Models;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Entities;
using WaitifyApi.Repositories;
using WaitifyApi.Enums;
using Newtonsoft.Json;
using System.Security.Claims;
using WaitifyApi.Exceptions;
using Microsoft.Extensions.Options;
using WaitifyApi.Constants;

namespace WaitifyApi.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    IAuthTokenProcessor authTokenProcessor,
    FileStorageService fileStorageService,
    IEmailRepository emailService,
    IOptions<PeopleApiPhotos> options,
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
        string url = AppConstants.Config.WaitifyUnsuscribeUrl;
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
        var GoogleProfilePicture = $"https://people.googleapis.com/v1/people/{claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)}?personFields=photos&key={GoogleApiKey}";

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
                ProfilePicture = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRglS-Qi4t9NLrCFtsFPv1vBYiVWzv1kvdemqQWVNVmuA&s=10",
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

            string url = AppConstants.Config.WaitifyUnsuscribeUrl;

            await emailService.RegisterEmail(user.Email, user.UserName, user.CreatedAt.ToString(), url);
            await emailService.NewUserAcquiredEmail(user.Email, user.UserName, user.Id, user.CreatedAt.ToString(), trialEndDate.ToString());
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

        using (HttpClient httpClient = new HttpClient())
        {
            string s = await httpClient.GetStringAsync(GoogleProfilePicture);
            dynamic deserializeObject = JsonConvert.DeserializeObject(s);
            string thumbnailUrl = (string)deserializeObject.photos[0].url;
            byte[] thumbnail = await httpClient.GetByteArrayAsync(thumbnailUrl);
            user.ProfilePicture = thumbnailUrl;
        }

        await userManager.UpdateAsync(user);

        logger.LogInformation("Google JWT Token : {@0}", jwtToken);

        authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("waitify_token", jwtToken, expirationDateInUtc);
        authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);

        return jwtToken;
    }

    public async Task<bool> SendPasswordResetLinkAsync(string email)
    {
        // Try to find the user by their email address
        var user = await userManager.FindByEmailAsync(email);
        var userEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);

        // Security measure:
        // Do not reveal whether the user exists or not —
        // always behave the same if the user is not found or the email is not confirmed

        if (user == null || !userEmailConfirmed)
            return false;

        // Generate a unique, secure token for password reset
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // Encode the token so it can be safely used in a URL
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Construct the password reset link with the encoded token and user’s email
        var baseUrl = _configuration["AppSettings:BaseUrl"];
        var resetLink = $"{baseUrl}/Account/ResetPassword?email={user.Email}&token={encodedToken}";

        // Send the reset link via email to the user
        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FirstName, resetLink);

        return true;
    }
}
