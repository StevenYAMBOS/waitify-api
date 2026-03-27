using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services
{
    public class BusinessService(AppDbContext context, IApplicationUserRepository userService, FileStorageService fileStorageService, ILogger<BusinessService> logger) : IBusinessRepository
    {
        public async Task<byte[]> CreateBusinessAsync(string userId, BusinessRequest request)
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
            Guid qrCodeToken = new Guid();

            if (request.Logo is not null)
            {
                string[] allowedExtensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
                logoUrl = await fileStorageService.UploadBlobAsync(request.Logo, businessName, azureContainerName, allowedExtensions);
            }


            var business = new Business
            {
                Name = request.Name,
                BusinessType = request.BusinessType,
                PhoneNumber = request.PhoneNumber,
                Logo = logoUrl,
                Address = request.Address,
                City = request.City,
                ZipCode = request.ZipCode,
                Country = request.Country,
                QrCodeToken = qrCodeToken.ToString(),
                CreatedAt = DateTime.UtcNow
            };


            context.Businesses.Add(business);
            await context.SaveChangesAsync();

            var url = AppConstants.WaitifyUrl + "/q/" + qrCodeToken;
            var qrCodeGenerated = QRCodeHelper.GenerateToFile(url);

            return qrCodeGenerated;
        }
    }
}