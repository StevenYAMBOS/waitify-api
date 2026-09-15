using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<GetBusinessLiveKpisResponse> GetLiveKpisAsync(Guid businessId, string userId);
    }
}
