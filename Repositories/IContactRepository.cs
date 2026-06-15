
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories;

public interface IContactRepository
{
    Task<Contact> SendContactInfoAsync(SendContactInfoDto request);
    Task<Contact?> FindContactByIdAsync(Guid contactId);
    Task<AdminGetAllWaitifyContactsResponse> AdminGetAllWaitifyContactsAsync(string userId);
    Task DeleteContatAsync(Guid contactId);

}