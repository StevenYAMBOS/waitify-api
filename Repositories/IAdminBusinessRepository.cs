

using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories;

public interface IAdminBusinessRepository
{
    Task<CreateBusinessResponse> CreateBusinessAsync(string userId, BusinessRequest request);
    Task<IEnumerable<Business>> FindBusinessByIdAsync(string businessId);
    Task<IEnumerable<Business>> GetAllBusinessesAsync();
    Task<IEnumerable<Business>> GetBusinessesOfUserAsync(string userId);
    Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
    Task<Business> UpdateBusinessLogoAsync(Guid businessId, UpdateBusinessLogoRequest request);
    Task<string> GenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken);
    Task<string> OpenOrCloseBusinessQueueAsync(Guid qrCodeToken, OpenOrCloseBusinessQueueRequest request);
    Task<(bool deleted, string Error)> DeleteBusinessAsync(Guid businessId);
}