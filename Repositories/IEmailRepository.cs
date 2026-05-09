
namespace WaitifyApi.Repositories;

public interface IEmailRepository
{
    Task RegisterEmail(string receiver, string userName, string createdAt, string url);
}