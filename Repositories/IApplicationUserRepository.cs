using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
namespace WaitifyApi.Repositories;

public interface IApplicationUserRepository
{
    Task<ApplicationUser?> FindUserByIdAsync(string id);
    // Task<IEnumerable<Business>> GetBusinessesAsync(string id);
    Task<(bool Success, ApplicationUser? User, string? Error)> UpdateProfilAsync(string id, JsonPatchDocument<ApplicationUser> patchDocument);
    Task DeleteProfilAsync(string id);
}