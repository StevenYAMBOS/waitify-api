using WaitifyApi.Entities;

namespace WaitifyApi.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken);
}