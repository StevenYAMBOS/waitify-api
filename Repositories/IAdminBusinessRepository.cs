

using WaitifyApi.Entities;

namespace WaitifyApi.Repositories;

public interface IAdminBusinessRepository
{
    Task<IEnumerable<Business>> FindBusinessByIdAsync(string businessId);
    Task<IEnumerable<Business>> GetAllBusinessesAsync();
    Task<IEnumerable<Business>> GetBusinessesOfUserAsync(string userId);
    Task<(bool deleted, string Error)> DeleteBusinessAsync(Guid businessId);

}