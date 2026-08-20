using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
namespace WaitifyApi.Repositories;

public interface IApplicationUserRepository
{
    Task<ApplicationUser?> FindUserByIdAsync(string id);
    Task<ApplicationUser?> FindUserByEmailAsync(string email);
    Task<(bool Success, ApplicationUser? User, string? Error)> UpdateProfilAsync(string id, JsonPatchDocument<ApplicationUser> patchDocument);
    Task DeleteProfilAsync(string id);
    Task<AdminDeleteUserResponse> AdminDeleteUserAsync(AdminDeleteUserRequest request);
}
