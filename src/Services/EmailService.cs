using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailRepository
{
    /* ************************* EMAILS ************************* */

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

    // Email envoyé aux développeurs
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

    // Email envoyé aux utilisateurs (formulaire de contact)
    public async Task SendContactEmail(string userEmail, string subject, DateTime createdAt)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser));
            email.To.Add(new MailboxAddress("Destinataire", userEmail));
            email.Subject = subject;

            string body = SendContactEmailBody(userEmail, subject, createdAt);

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
            logger.LogInformation("Erreur lors de l'envoi de l'email : {@0}", ex.Message);
        }
    }

    public async Task SendResetPasswordEmail(string firstName, string resetLink)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser));
            email.To.Add(new MailboxAddress("Destinataire", userEmail));
            email.Subject = subject;

            string body = ResetPasswordEmailBody(firstName, resetLink);

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
            logger.LogInformation("Erreur lors de l'envoi de l'email : {@0}", ex.Message);
        }
    }

    public async Task SendPasswordUpdatedEmail(string userEmail, string firstName, DateTime updatedAt)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("SmtpPort")!);
            var smtpUser = Environment.GetEnvironmentVariable("SmtpUser");
            var password = Environment.GetEnvironmentVariable("SmtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser));
            email.To.Add(new MailboxAddress("Destinataire", userEmail));
            email.Subject = "Waitify - Votre mot de passe a été modifié";

            string body = SendPasswordUpdatedEmailBody(firstName, updatedAt);

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpServer!, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser!, password!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur lors de l'envoi de l'email : {@0}", ex.Message);
        }
    }

    /* ************************* CORPS DES EMAILS ************************* */
    // Corps des emails utilisés par les template HTML

    private static string NewUserAcquiredEmailBody(string userEmail, string userName, string userId, string createdAt, string trialEndsAt)
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

    private static string RegisterEmailBody(string userName, string receiver, string createdAt, string url)
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

    private static string SendContactEmailBody(string userEmail, string subject, DateTime createdAt)
    {
        string body;
        using (StreamReader sourceReader = File.OpenText("Utils/RequestContactEmail.html"))
        {
            body = sourceReader.ReadToEnd();
        }

        body = body.Replace("{userEmail}", userEmail)
                   .Replace("{subject}", subject)
                   .Replace("{createdAt}", createdAt.ToString("dd/MM/yyyy HH:mm"));

        return body;
    }

    private static string ResetPasswordEmailBody(string firstName, string resetLink)
    {
        string body;
        using (StreamReader sourceReader = File.OpenText("Utils/ResetPasswordEmail.html"))
        {
            body = sourceReader.ReadToEnd();
        }

        body = body.Replace("{firstName}", firstName)
                   .Replace("{resetLink}", resetLink);
        return body;
    }

    private static string SendPasswordUpdatedEmailBody(string firstName, DateTime updatedAt)
    {
        string body;
        using (StreamReader sourceReader = File.OpenText("Utils/PasswordUpdatedEmail.html"))
        {
            body = sourceReader.ReadToEnd();
        }

        body = body.Replace("{firstName}", firstName)
                   .Replace("{updatedAt}", updatedAt.ToString("dd/MM/yyyy HH:mm"));

        return body;
    }
}
