using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        // Task<IEnumerable<Business>> GetAllPubishedBusinessesAsync();
        // Task<IEnumerable<Business>> GetAllBusinessesAsync();
        // Task<Business?> FindBusinessByIdAsync(Guid id);
        Task<Business> CreateBusinessAsync(string userId, BusinessRequest request);
        // Task<Business> UpdateBusinessAsync(Guid articleId, UpdateBusinessDTO request);
        // Task<Business> TogglePublishBusinessAsync(Guid articleId, bool isPublished, string authorIdFromToken);
        // Task DeleteBusinessAsync(Guid id);
    }
}