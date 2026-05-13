

using Microsoft.EntityFrameworkCore;
using WaitifyApi.Data;
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class ContactService(AppDbContext context, FileStorageService fileService, IEmailRepository emailService, ILogger<ContactService> logger) : IContactRepository
{
    public async Task<Contact> SendContactInfoAsync(SendContactInfoDto request)
    {
        string? fileUrl = null;
        string contactName = request?.Email;
        string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobContactContainer")!;

        if (request.File is not null)
        {
            string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
            fileUrl = await fileService.UploadBlobAsync(request.File, contactName, azureContainerName, allowedExtensions);
        }

        var contact = new Contact
        {
            Email = request.Email,
            Subject = request.Subject,
            Content = request.Content,
            File = fileUrl,
            CreatedAt = DateTime.UtcNow,
        };

        await emailService.SendContactEmail(request?.Email, request?.Subject, request?.Content, request?.File);
        logger.LogInformation("Email '{0}' envoyé avec succès : ", request.File);


        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        return contact;
    }

    public async Task<Contact?> FindContactByIdAsync(Guid id)
    {
        var contact = await context.Contacts.FindAsync(id);
        logger.LogInformation("Information de la demande : {0}", contact);
        return contact;
    }

    public async Task<IEnumerable<Contact>> GetContactsAsync()
    {
        var contacts = await context.Contacts.ToListAsync();
        logger.LogInformation("LISTE DES DEMANDES : {@0}", contacts);
        return contacts;
    }

    public async Task DeleteContatAsync(Guid contactId)
    {
        var contact = await FindContactByIdAsync(contactId) ?? throw new KeyNotFoundException("Demande non trouvée.");

        if (contact.File != null)
        {
            string blobUrl = Environment.GetEnvironmentVariable("AzureGenericBlobsUrl")!;
            string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobContactContainer")!;
            string blobFileName = contact.File.Replace(blobUrl + azureContainerName, "");
            await fileService.DeleteBlobSnapshotsAsync(blobFileName, azureContainerName);
        }

        context.Contacts.Remove(contact);
        await context.SaveChangesAsync();
    }
}