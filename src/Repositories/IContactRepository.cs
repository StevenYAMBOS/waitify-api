using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Models;

namespace WaitifyApi.Repositories;

public interface IContactRepository
{
    Task<Contact> SendContactInfoAsync(SendContactInfoRequest request);
    Task<Contact?> FindContactByIdAsync(Guid contactId);
    Task<AdminGetAllWaitifyContactsResponse> AdminGetAllWaitifyContactsAsync();
    Task<AdminGetAllWaitifyContactsResponse> AdminFindContactsListByUserAsync(string userId);
    Task<AdminDeleteContactResponse> AdminDeleteContatAsync(Guid contactId);

}
