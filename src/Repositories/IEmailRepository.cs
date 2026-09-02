
namespace WaitifyApi.Repositories;

public interface IEmailRepository
{
    Task RegisterEmail(string receiver, string userName, string createdAt, string url);
    Task NewUserAcquiredEmail(string userEmail, string userName, string userId, string createdAt, string trialEndsAt);
    Task AlertContactFormEmail(Guid contactId, string userEmail, string subject, string content, DateTime createdAt);
    Task SendContactEmail(string userEmail, string subject, DateTime createdAt);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetPasswordLink);
}
