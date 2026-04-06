using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class BusinessService(AppDbContext context, IApplicationUserRepository userService, QRCodeGeneratorService qRCodeHelper, FileStorageService fileStorageService, ILogger<BusinessService> logger) : IBusinessRepository
{
    public async Task<string> GenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken)
    {
        var business = await FindBusinessByIdAsync(businessId);
        if (business == null)
        {
            logger.LogError("Entreprise non trouvée.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", business?.Id, businessId);
            throw new KeyNotFoundException("Entreprise non trouvée.");
        }

        var existingUser = userService.FindUserByIdAsync(userId);
        if (existingUser?.Id.ToString() == userId)
        {
            logger.LogError("Accès interdit. L'id utilisateur est incorrecte.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", existingUser?.Id, userId);
            throw new KeyNotFoundException("Utilisateur non trouvé ou accès non autorisé.");
        }

        if (userId != business.OwnerId)
        {
            logger.LogError("Accès refusé !\n ID récupéré du JWT : `{@0}`.\n ID du gérant en BDD : `{@1}`.", userId, business.OwnerId);
            throw new KeyNotFoundException("Utilisateur non trouvé.");
        }

        var url = AppConstants.WaitifyUrl + "/q/" + qrCodeToken;
        var qrCodeGenerated = await qRCodeHelper.GenerateQRCode(url);

        logger.LogInformation("Nouveau QRCode généré : {@0}", JsonConvert.SerializeObject(qrCodeGenerated, Formatting.Indented));
        return qrCodeGenerated;
    }

    /*     public async Task<Business?> GetBusinessByIdWithActiveQueueAsync(Guid id)
        {
            FormattableString query = $"SELECT id, is_queue_active, max_queue_size, average_service_time FROM businesses WHERE BusinessId = {id} AND is_active = TRUE";
            var business = await context.Businesses.FromSql(query)
            .Where(b => b.Id == id)
            .ToListAsync();

            return business.FirstOrDefault();
        } */

    public async Task<Business?> FindBusinessByIdAsync(Guid id)
    {
        var business = await context.Businesses.FindAsync(id);
        return business;
    }

    public async Task<Business?> FindBusinessByQrTokenAsync(Guid qrCodeToken)
    {
        return await context.Businesses.FirstOrDefaultAsync(b => b.QrCodeToken == qrCodeToken);
    }

    public async Task<string> CreateBusinessAsync(string userId, BusinessRequest request)
    {
        var user = await userService.FindUserByIdAsync(userId);
        if (user == null)
        {
            logger.LogError("L'id utilisateur n'est pas correcte : {@0}", user.Id);
            throw new KeyNotFoundException("Utilisateur non trouvé");
        }

        string? logoUrl = null;
        string businessName = $"{request.Name}";
        string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobBusinessesContainer")!;
        Guid qrCodeToken = Guid.NewGuid();

        if (request.Logo is not null)
        {
            string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
            logoUrl = await fileStorageService.UploadBlobAsync(request.Logo, businessName, azureContainerName, allowedExtensions);
        }

        var business = new Business
        {
            OwnerId = user.Id,
            Name = request.Name,
            BusinessType = request.BusinessType,
            PhoneNumber = request.PhoneNumber,
            Logo = logoUrl,
            Address = request.Address,
            City = request.City,
            ZipCode = request.ZipCode,
            Country = request.Country,
            QrCodeToken = qrCodeToken,
            CreatedAt = DateTime.UtcNow
        };

        context.Businesses.Add(business);
        await context.SaveChangesAsync();

        var url = AppConstants.WaitifyUrl + "/q/" + qrCodeToken;
        var qrCodeGenerated = await qRCodeHelper.GenerateQRCode(url);

        logger.LogInformation("ID entreprise : {@0}", business.Id);
        return qrCodeGenerated;
    }

    public async Task<(bool Success, Business? Business, string? Error)> UpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument)
    {
        try
        {
            var existingBusiness = context.Businesses.FirstOrDefault(business => business.Id == businessId);
            if (existingBusiness == null)
            {
                logger.LogError("Erreur lors de la mise à jour de l'entreprise.");
                return (false, null, "Erreur lors de la mise à jour de l'entreprise.");
            }

            patchDocument.ApplyTo(existingBusiness);
            existingBusiness.UpdatedAt = DateTime.UtcNow;

            context.Businesses.Update(existingBusiness);
            await context.SaveChangesAsync();

            logger.LogInformation("Entreprise mis à jour avec succès : {@0}", JsonConvert.SerializeObject(existingBusiness, Formatting.Indented));
            return (true, existingBusiness, null);
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur : {@0}", ex);
            throw new InvalidOperationException("Une erreur est survenue lors de la mise à jour de l'entreprise.", ex);
        }
    }
    public async Task<Business?> UpdateBusinessLogoAsync(
        // string userId, 
        Guid businessId,
        UpdateBusinessLogoRequest request
        )
    {
        // var existingUser = userService.FindUserByIdAsync(userId);
        // if (existingUser?.Id.ToString() == userId)
        // {
        //     logger.LogError("Accès interdit. L'id utilisateur est incorrecte.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", existingUser?.Id, userId);
        //     return null;
        // }

        try
        {
            var existingBusiness = context.Businesses.FirstOrDefault(business => business.Id == businessId);
            if (existingBusiness == null)
            {
                logger.LogError("Entreprise non trouvée.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", existingBusiness?.Id, businessId);
                return null;
            }

            // if (existingUser?.Id.ToString() != existingBusiness.OwnerId)
            // {
            //     logger.LogError("Accès refusé !\n ID récupéré du JWT : `{@0}`.\n ID du gérant en BDD : `{@1}`.", existingUser?.Id, existingBusiness.OwnerId);
            //     return null;
            // }

            string oldImage = existingBusiness?.Logo;
            string? logoUrl = null;
            string businessName = $"{existingBusiness.Name}";
            string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobBusinessesContainer")!;
            Guid qrCodeToken = Guid.NewGuid();

            if (request.NewLogoFile is not null)
            {
                string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
                logoUrl = await fileStorageService.UploadBlobAsync(request.NewLogoFile, businessName, azureContainerName, allowedExtensions);
            }

            existingBusiness.Logo = request.NewLogoFile != null ? logoUrl : existingBusiness.Logo;
            existingBusiness.UpdatedAt = DateTime.UtcNow;

            context.Businesses.Update(existingBusiness);
            await context.SaveChangesAsync();

            if (request.NewLogoFile != null && oldImage != null)
            {
                string blobUrl = Environment.GetEnvironmentVariable("AzureGenericBlobsUrl")!;
                string blobFileName = existingBusiness?.Logo.Replace(blobUrl + azureContainerName, "");
                await fileStorageService.DeleteBlobSnapshotsAsync(blobFileName, azureContainerName);
            }

            logger.LogInformation("Logo mis à jour avec succès !");

            return existingBusiness;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Une erreur est survenue lors de la mise à jour du logo de l'entreprise.", ex);
        }
    }

    public async Task<string> OpenOrCloseBusinessQueueAsync(Guid idBusiness, bool switchStatus)
    {
        try
        {
            var existingBusiness = await FindBusinessByIdAsync(idBusiness);

            if (existingBusiness == null)
            {
                logger.LogError("Entreprise non trouvée : `{@0}`.", existingBusiness?.Id);
                throw new KeyNotFoundException("Entreprise non trouvée.");
            }

            existingBusiness.IsQueueActive = switchStatus;
            existingBusiness.UpdatedAt = DateTime.UtcNow;

            context.Businesses.Update(existingBusiness);
            await context.SaveChangesAsync();

            logger.LogInformation("Status de la file d'attente de l'entreprise mis à jour avec succès : {@0}", JsonConvert.SerializeObject(existingBusiness.IsQueueActive, Formatting.Indented));
            return "Status de la file d'attente de l'entreprise mis à jour avec succès";
        }
        catch (Exception ex)
        {
            logger.LogInformation("Erreur : {@0}", ex);
            throw new InvalidOperationException("Une erreur est survenue lors de la mise à jour de l'entreprise.", ex);
        }
    }

    public async Task DeleteBusinessAsync(Guid id)
    {
        var business = await FindBusinessByIdAsync(id) ?? throw new KeyNotFoundException("Entreprise non trouvée.");

        if (business.Logo != null)
        {
            string blobUrl = Environment.GetEnvironmentVariable("AzureGenericBlobsUrl")!;
            string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobBusinessesContainer")!;
            string blobFileName = business.Logo.Replace(blobUrl + azureContainerName, "");
            await fileStorageService.DeleteBlobSnapshotsAsync(blobFileName, azureContainerName);
        }

        context.Businesses.Remove(business);
        await context.SaveChangesAsync();
    }
}