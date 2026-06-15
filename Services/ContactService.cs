

using Microsoft.EntityFrameworkCore;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Enums;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class ContactService(AppDbContext context, FileStorageService fileService, IEmailRepository emailService, ApplicationUserService userService, ILogger<ContactService> logger) : IContactRepository
{
    public async Task<Contact> SendContactInfoAsync(SendContactInfoDto request)
    {
        string? fileUrl = null;
        string contactName = request?.Email;
        string azureContainerName = AppConstants.Azure.ContactsContainer;

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

    public async Task<AdminGetAllWaitifyContactsResponse> AdminGetAllWaitifyContactsAsync(string userId)
    {
        var user = await userService.FindUserByIdAsync(userId);
        var role = user?.Role;
        var contacts = await context.Contacts.ToListAsync();
        var contactsCount = await context.Contacts.CountAsync();

        if (role != Role.Admin)
        {
            logger.LogInformation(AppConstants.Authorization.Denied);
            throw new ArgumentException(AppConstants.Authorization.Denied);
        }

        var response = new AdminGetAllWaitifyContactsResponse
        {
            Count = contactsCount,
            Contacts = contacts
        };

        logger.LogInformation("LISTE DES DEMANDES : {@0}", JsonResponseHelper.JsonConversion(contacts));
        return response;
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