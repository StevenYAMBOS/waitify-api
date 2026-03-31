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
    public async Task<string> GenerateNewQRCodeAsync(Guid qrCodeToken)
    {
        var url = AppConstants.WaitifyUrl + "/q/" + qrCodeToken;
        var qrCodeGenerated = await qRCodeHelper.GenerateQRCode(url);

        logger.LogInformation("Nouveau QRCode généré : {@0}", JsonConvert.SerializeObject(qrCodeGenerated, Formatting.Indented));
        return qrCodeGenerated;
    }

    public async Task<Business?> FindBusinessByIdAsync(Guid id)
    {
        var business = await context.Businesses.FindAsync(id);
        return business;
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

        // QRCodeGenerator qrGenerator = new QRCodeGenerator();
        // QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        // PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        // byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(5);
        // string base64String = Convert.ToBase64String(qrCodeAsPngByteArr, 0, qrCodeAsPngByteArr.Length);
        // var qrCodeGenerated = $"<img src='data:image/png;base64,{base64String}' />";

        logger.LogInformation("ID entreprise : {@0}", business.Id);
        return qrCodeGenerated;
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