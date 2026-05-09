
namespace WaitifyApi.Repositories;

public interface IEmailRepository
{
    Task RegisterEmail(string receiver, string userName, string createdAt, string url);
    Task NewUserAcquiredEmail(string userEmail, string userName, string userId, string createdAt, string trialEndsAt);
}