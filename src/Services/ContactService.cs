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

public class ContactService(AppDbContext context, FileStorageService fileService, IEmailRepository emailService, IApplicationUserRepository userService, ILogger<ContactService> logger) : IContactRepository
{
    public async Task<Contact> SendContactInfoAsync(SendContactInfoRequest request)
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


        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        await emailService.AlertContactFormEmail(contact.Id, contact.Email, contact.Subject, contact.Content, contact.CreatedAt);
        await emailService.SendContactEmail(contact.Email, contact.Subject, contact.CreatedAt);
        logger.LogInformation("Emails '{@0}' envoyés avec succès", contact.Subject);

        return contact;
    }

    public async Task<Contact?> FindContactByIdAsync(Guid id)
    {
        var contact = await context.Contacts.FindAsync(id);
        logger.LogInformation("Information de la demande : {@0}", contact);
        return contact;
    }

    public async Task<AdminGetAllWaitifyContactsResponse> AdminGetAllWaitifyContactsAsync()
    {
        var contacts = await context.Contacts.ToListAsync();
        var contactsCount = await context.Contacts.CountAsync();

        var response = new AdminGetAllWaitifyContactsResponse
        {
            Count = contactsCount,
            Contacts = contacts
        };

        logger.LogInformation("LISTE DES DEMANDES : {@0}", JsonResponseHelper.JsonConversion(contacts));
        return response;
    }

    public async Task<AdminGetAllWaitifyContactsResponse> AdminFindContactsListByUserAsync(string userId)
    {
        var user = await userService.FindUserByIdAsync(userId) ?? throw new KeyNotFoundException("Utilisateur non trouvé.");

        var contactsCount = await context.Contacts
        .Where(
          contact => contact.Email == user.Email)
        .CountAsync();

        logger.LogInformation("Nombre de demandes de l'utilisateur `{@0}` : {@1}", user.Email, JsonResponseHelper.JsonConversion(contactsCount));

        var contacts = await context.Contacts.ToListAsync();

        var response = new AdminGetAllWaitifyContactsResponse
        {
            Count = contactsCount,
            Contacts = contacts
        };

        logger.LogInformation("LISTE DES DEMANDES : {@0}", JsonResponseHelper.JsonConversion(contacts));
        return response;
    }

    public async Task<Contact?> AdminValideContactAsync(Guid contactId)
    {
        try
        {
            var contact = await FindContactByIdAsync(contactId);
            if (contact == null)
            {
                logger.LogError("Demande avec cet Id introuvable -> `{@0}`.", contactId);
                throw new KeyNotFoundException("Demande non trouvée");
            }

            contact.Checked = true;
            contact.UpdatedAt = DateTime.UtcNow;

            context.Contacts.Update(contact);
            await context.SaveChangesAsync();

            logger.LogInformation("Demande validée avec succès : `{@0}`.", contact);
            return contact;
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur : {@0}", ex);
            throw new InvalidOperationException("Une erreur est survenue lors de la mise à jour de la demande.", ex);
        }
    }

    public async Task<AdminDeleteContactResponse> AdminDeleteContatAsync(Guid contactId)
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

        var response = new AdminDeleteContactResponse
        {
            Success = true,
            Message = "Suppression réussie."
        };

        logger.LogInformation("Contact '{@0}' supprimé avec succès.", contactId);
        return response;
    }
}
