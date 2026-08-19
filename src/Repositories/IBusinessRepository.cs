using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        Task<Business?> FindBusinessByIdAsync(Guid businessId, string userId);
        Task<Business?> FindBusinessByQrTokenAsync(Guid qrCodeToken);
        Task<IEnumerable<Business>> GetAllOwnerBusinessesAsync(string businessId);
        Task<GetAllWaitifyBusinessesResponse> GetAllWaitifyBusinessesAsync(string userId);
        Task<string> CreateBusinessAsync(string userId, BusinessRequest request);
        Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
        Task<Business?> UpdateBusinessLogoAsync(Guid businessId, UpdateBusinessLogoRequest request);
        Task DeleteBusinessAsync(Guid businessId, string userId);
        Task<string> GenerateNewQRCodeAsync(Guid businessQRCodeToken, string userId);
        Task<string> OpenOrCloseBusinessQueueAsync(Guid qrCodeToken, OpenOrCloseBusinessQueueRequest request);
    }
}
