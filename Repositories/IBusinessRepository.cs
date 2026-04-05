using System.Drawing;
using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        Task<Business?> FindBusinessByIdAsync(Guid id);
        Task<Business?> FindBusinessByQrTokenAsync(Guid qrCodeToken);
        Task<string> CreateBusinessAsync(string userId, BusinessRequest request);
        Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
        Task<Business?> UpdateBusinessLogoAsync(
            // string userId, 
            Guid businessId,
            UpdateBusinessLogoRequest request);
        Task DeleteBusinessAsync(Guid id);
        Task<string> GenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken);
        // Task<IEnumerable<Business>> GetAllBusinessesAsync();
        // Task<IEnumerable<Business>> GetAllPubishedBusinessesAsync();
        // Task<Business> TogglePublishBusinessAsync(Guid articleId, bool isPublished, string authorIdFromToken);
    }
}