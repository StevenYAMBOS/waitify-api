

using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories;

public interface IAdminBusinessRepository
{
    Task<string> AdminCreateBusinessAsync(string userId, AdminBusinessRequest request);
    Task<Business?> AdminFindBusinessByIdAsync(string businessId);
    Task<IEnumerable<Business>> AdminGetAllBusinessesAsync();
    Task<IEnumerable<Business>> AdminGetBusinessesOfUserAsync(string userId);
    Task<(bool Success, Business? Business, string? Error)> AdminUpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
    Task<Business> AdminUpdateBusinessLogoAsync(Guid businessId, AdminUpdateBusinessLogoRequest request);
    Task<string> AdminGenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken);
    Task<string> AdminOpenOrCloseBusinessQueueAsync(Guid qrCodeToken, AdminOpenOrCloseBusinessQueueRequest request);
    Task<(bool deleted, string Error)> AdminDeleteBusinessAsync(Guid businessId);
}