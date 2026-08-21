using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailRepository
{
    public async Task RegisterEmail(string receiver, string userName, string createdAt, string url)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort"));
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser!));
            email.To.Add(new MailboxAddress("Destinataire", receiver));
            email.Subject = "Bienvenue chez Waitify !";

            string body = RegisterEmailBody(userName, receiver, createdAt, url);

            var builder = new BodyBuilder()
            {
                HtmlBody = body
            }
            ;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(smtpServer!, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser!, password!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur lors de l'envoi du mail : {0}", ex.Message);
        }
    }

    public async Task NewUserAcquiredEmail(string userEmail, string userName, string userId, string createdAt, string trialEndsAt)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser!));
            email.To.Add(new MailboxAddress("Destinataire", smtpUser!));
            email.Subject = "WAITIFY - Nouvelle inscription !";

            string body = NewUserAcquiredEmailBody(userEmail, userName, userId, createdAt, trialEndsAt);

            var builder = new BodyBuilder()
            {
                HtmlBody = body
            }
            ;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(smtpServer!, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser!, password!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur lors de l'envoi du mail : {@0}", ex.Message);
            // return $"Erreur lors de l'envoi du mail : {ex.Message}";
        }
    }

    private string NewUserAcquiredEmailBody(string userEmail, string userName, string userId, string createdAt, string trialEndsAt)
    {
        string body = string.Empty;
        using (StreamReader SourceReader = File.OpenText("Utils/NewUserAcquiredEmail.html"))
        {
            body = SourceReader.ReadToEnd();
        }

        body = body.Replace("{userId}", userId);
        body = body.Replace("{userEmail}", userEmail);
        body = body.Replace("{trialEndsAt}", trialEndsAt);
        body = body.Replace("{userName}", userName);
        body = body.Replace("{createdAt}", createdAt);
        return body;
    }

    private string RegisterEmailBody(string userName, string receiver, string createdAt, string url)
    {
        string body = string.Empty;
        using (StreamReader SourceReader = File.OpenText("Utils/RegisterEmail.html"))
        {
            body = SourceReader.ReadToEnd();
        }

        body = body.Replace("{userName}", userName);
        body = body.Replace("{receiver}", receiver);
        body = body.Replace("{createdAt}", createdAt);
        body = body.Replace("{url}", url);
        return body;
    }

    public async Task AlertContactFormEmail(Guid contactId, string userEmail, string subject, string content, DateTime createdAt)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("Waitify.fr", smtpUser!));
            mail.To.Add(new MailboxAddress("Destinataire", smtpUser!));
            mail.Subject = "WAITIFY - Formulaire de contact !";

            string body = AlertContactFormEmailBody(contactId, userEmail, subject, content, createdAt);

            var builder = new BodyBuilder { HtmlBody = body };
            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpServer!, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser!, password!);
            await smtp.SendAsync(mail);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur lors de l'envoi du mail : {@0}", ex.Message);
        }
    }

    private static string AlertContactFormEmailBody(Guid contactId, string userEmail, string subject, string content, DateTime createdAt)
    {
        string body;
        using (StreamReader sourceReader = File.OpenText("Utils/AlertContactFormEmail.html"))
        {
            body = sourceReader.ReadToEnd();
        }

        body = body.Replace("{contactId}", contactId.ToString())
                   .Replace("{userEmail}", userEmail)
                   .Replace("{subject}", subject)
                   .Replace("{content}", content)
                   .Replace("{createdAt}", createdAt.ToString("dd/MM/yyyy HH:mm"));

        return body;
    }

    // Email envoyé aux développeurs
    // public async Task SendContactEmail(string sender, string subject, string body, IFormFile file)
    // {
    //     try
    //     {
    //         var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
    //         var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
    //         var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
    //         var password = Environment.GetEnvironmentVariable("SmtpPassword");

    //         var email = new MimeMessage();
    //         email.From.Add(new MailboxAddress("stevenyambos.fr", sender));
    //         email.To.Add(new MailboxAddress("Destinataire", smtpUser));
    //         email.Subject = subject;

    //         var builder = new BodyBuilder
    //         {
    //             TextBody = body
    //         };

    //         if (email.Attachments != null)
    //         {
    //             string fileName = Path.GetFileName(file.FileName);
    //             builder.Attachments.Add(fileName, file.OpenReadStream());
    //         }
    //         email.Body = builder.ToMessageBody();

    //         using var smtp = new SmtpClient();

    //         await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
    //         await smtp.AuthenticateAsync(smtpUser, password);
    //         await smtp.SendAsync(email);
    //         await smtp.DisconnectAsync(true);
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogInformation("Erreur lors de l'envoi du mail : {@0}", ex.Message);
    //         // return $"Erreur lors de l'envoi du mail : {ex.Message}";
    //     }
    // }
}
