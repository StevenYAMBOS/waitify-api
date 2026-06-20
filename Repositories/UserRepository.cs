using Microsoft.EntityFrameworkCore;
using WaitifyApi.Data;
using WaitifyApi.Entities;

namespace WaitifyApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _applicationDbContext;

    public UserRepository(AppDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        return user;
    }
}