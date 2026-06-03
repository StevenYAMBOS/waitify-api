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

public class AdminBusinessService(AppDbContext context, IApplicationUserRepository userService, QRCodeGeneratorService qRCodeHelper, FileStorageService fileStorageService, ILogger<BusinessService> logger) : IAdminBusinessRepository
{
    public async Task<string> AdminCreateBusinessAsync(string userId, AdminBusinessRequest request)
    {
        var user = await userService.FindUserByIdAsync(userId);
        if (user == null)
        {
            logger.LogError("L'id utilisateur n'est pas correcte : {@0}", user?.Id);
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

    public Task<(bool deleted, string Error)> AdminDeleteBusinessAsync(Guid businessId)
    {
        throw new NotImplementedException();
    }

    public Task<Business?> AdminFindBusinessByIdAsync(string businessId)
    {
        throw new NotImplementedException();
    }

    public Task<string> AdminGenerateNewQRCodeAsync(Guid businessId, string userId, Guid qrCodeToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Business>> AdminGetAllBusinessesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Business>> AdminGetBusinessesOfUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<string> AdminOpenOrCloseBusinessQueueAsync(Guid qrCodeToken, AdminOpenOrCloseBusinessQueueRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, Business? Business, string? Error)> AdminUpdateBusinessAsync(Guid businessId, JsonPatchDocument<Business> patchDocument)
    {
        throw new NotImplementedException();
    }

    public Task<Business> AdminUpdateBusinessLogoAsync(Guid businessId, AdminUpdateBusinessLogoRequest request)
    {
        throw new NotImplementedException();
    }
}