using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
namespace WaitifyApi.Repositories;

public interface IUserProfileService
{
    Task<ApplicationUser?> FindUserByIdAsync(string id);
    Task<(bool Success, ApplicationUser? User, string? Error)> UpdateProfilAsync(string id, JsonPatchDocument<ApplicationUser> patchDocument);
    Task DeleteProfilAsync(string id);
}