
namespace WaitifyApi.Repositories;

public interface IEmailRepository
{
    Task RegisterEmail(string receiver);
}