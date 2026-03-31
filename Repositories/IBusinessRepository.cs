using System.Drawing;
using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IBusinessRepository
    {
        Task<string> GenerateNewQRCodeAsync(Guid qrCodeToken);
        Task<Business?> FindBusinessByIdAsync(Guid id);
        Task<string> CreateBusinessAsync(string userId, BusinessRequest request);
        Task DeleteBusinessAsync(Guid id);
        // Task<IEnumerable<Business>> GetAllBusinessesAsync();
        // Task<IEnumerable<Business>> GetAllPubishedBusinessesAsync();
        // Task<Business> UpdateBusinessAsync(Guid articleId, UpdateBusinessDTO request);
        // Task<Business> TogglePublishBusinessAsync(Guid articleId, bool isPublished, string authorIdFromToken);
    }
}