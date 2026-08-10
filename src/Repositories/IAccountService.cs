using System.Security.Claims;

namespace WaitifyApi.Repositories;

public interface IAccountService
{
    Task RefreshTokenAsync(string? refreshToken);
    Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal);
}