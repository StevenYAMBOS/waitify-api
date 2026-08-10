using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories;

public interface IAuthTokenProcessor
{
    (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(ApplicationUser user);
    string GenerateRefreshToken();
    void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token,
        DateTime expiration);
}
