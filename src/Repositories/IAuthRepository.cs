
using System.Security.Claims;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IAuthRepository
    {
        Task<(bool Success, string? Token, IEnumerable<string>? Errors)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, TokenResponseDTO? Tokens, string? Error)> LoginAsync(LoginRequest request);
        Task<(bool Success, TokenResponseDTO? Tokens, string? Error)> RefreshTokensAsync(RefreshTokenRequestDTO request);
        Task<string> LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal);
    }
}
