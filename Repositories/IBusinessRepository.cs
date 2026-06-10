using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        Task<Business?> FindBusinessByIdAsync(Guid businessId);
        Task<Business?> FindBusinessByQrTokenAsync(Guid qrCodeToken);
        Task<IEnumerable<Business>> GetAllOwnerBusinessesAsync(string businessId);
        Task<string> CreateBusinessAsync(string userId, BusinessRequest request);
        Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
        Task<Business?> UpdateBusinessLogoAsync(Guid businessId, UpdateBusinessLogoRequest request);
        Task DeleteBusinessAsync(Guid businessId);
        Task<string> GenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken);
        Task<string> OpenOrCloseBusinessQueueAsync(Guid qrCodeToken, OpenOrCloseBusinessQueueRequest request);
    }
}