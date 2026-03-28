using System.Drawing;
using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        Task<Business?> FindBusinessByIdAsync(Guid id);
        Task<string> CreateBusinessAsync(string userId, BusinessRequest request);
        Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument);
        Task<Business?> UpdateBusinessLogoAsync(string userId, Guid businessId, UpdateBusinessLogoRequest request);
        Task DeleteBusinessAsync(Guid id);
        // Task<IEnumerable<Business>> GetAllPubishedBusinessesAsync();
        // Task<IEnumerable<Business>> GetAllBusinessesAsync();
        // Task<Business> TogglePublishBusinessAsync(Guid articleId, bool isPublished, string authorIdFromToken);
    }
}