
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class EmailService(ILogger<EmailService> logger, IWebHostEnvironment environment) : IEmailRepository
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

            string body = PopulateBody(userName, receiver, createdAt, url);

            var builder = new BodyBuilder()
            {
                HtmlBody = body
            }
            ;
            email.Body = builder.ToMessageBody();

            /*
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText("Utils/RegisterEmail.html"))
                builder.HtmlBody = SourceReader.ReadToEnd();

            email.Body = builder.ToMessageBody();
            */

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

    private string PopulateBody(string userName, string receiver, string createdAt, string url)
    {
        string body = string.Empty;
        string path = Path.Combine(environment.WebRootPath, "Utils/RegisterEmail.html");
        using (StreamReader reader = new StreamReader(path))
        {
            body = reader.ReadToEnd();
        }
        body = body.Replace("{userName}", userName);
        body = body.Replace("{receiver}", receiver);
        body = body.Replace("{createdAt}", createdAt);
        body = body.Replace("{url}", url);
        return body;
    }
}