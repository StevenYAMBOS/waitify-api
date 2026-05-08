
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailRepository
{
    public async Task RegisterEmail(string receiver)
    {
        try
        {
            var smtpServer = Environment.GetEnvironmentVariable("smtpServer");
            var port = int.Parse(Environment.GetEnvironmentVariable("smtpPort"));
            var smtpUser = Environment.GetEnvironmentVariable("smtpUser");
            var password = Environment.GetEnvironmentVariable("smtpPassword");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Waitify.fr", smtpUser!));
            email.To.Add(new MailboxAddress("Destinataire", receiver));
            email.Subject = "Bienvenue chez Waitify !";
            email.Body = new TextPart("html") { Text = "TEMPLATE" };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(smtpServer!, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser!, password!);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur lors de l'envoi du mail : {0}", ex.Message);
            // return $"Erreur lors de l'envoi du mail : {ex.Message}";
        }
    }
}