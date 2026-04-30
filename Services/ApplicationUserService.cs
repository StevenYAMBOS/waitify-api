using Microsoft.AspNetCore.JsonPatch;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services
{
    public class ApplicationUserService(AppDbContext context, FileStorageService fileStorageService, ILogger<ApplicationUserService> logger) : IApplicationUserRepository
    {
        public async Task<ApplicationUser?> FindUserByIdAsync(string id)
        {
            var user = await context.Users.FindAsync(id);
            return user;
        }

        public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
        {
            var user = await context.Users.FindAsync(email);
            return user;
        }

        public async Task<(bool Success, ApplicationUser? User, string? Error)> UpdateProfilAsync(string id, JsonPatchDocument<ApplicationUser> patchDocument)
        {
            try
            {
                var existingUser = context.Users.FirstOrDefault(user => user.Id == id);
                if (existingUser == null)
                {
                    logger.LogError("Erreur lors de la mise à jour de l'utilisateur.");
                    return (false, null, "Erreur lors de la mise à jour de l'utilisateur.");
                }

                logger.LogInformation("Utilisateur mis à jour avec succès : {@0}", existingUser);
                patchDocument.ApplyTo(existingUser);

                context.Users.Update(existingUser);
                await context.SaveChangesAsync();

                return (true, existingUser, null);
            }
            catch (Exception ex)
            {
                logger.LogError("Erreur : {@0}", ex);
                throw new InvalidOperationException("Une erreur est survenue lors de la mise à jour de l'utilisateur.", ex);
            }
        }

        public async Task DeleteProfilAsync(string id)
        {
            var user = await FindUserByIdAsync(id) ?? throw new KeyNotFoundException("Utilisateur non trouvé.");

            if (user.ProfilePicture != null)
            {
                string blobUrl = Environment.GetEnvironmentVariable("AzureGenericBlobsUrl")!;
                string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobUsersContainer")!;
                string blobFileName = user.ProfilePicture.Replace(blobUrl + azureContainerName, "");
                await fileStorageService.DeleteBlobSnapshotsAsync(blobFileName, azureContainerName);
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        /*         public async Task<IEnumerable<Business>> GetBusinessesAsync(string id)
                {
                    var user = await FindUserByIdAsync(id) ?? throw new KeyNotFoundException("Utilisateur non trouvé.");
                    var businesses = await businessService.GetAllBusinessesAsync(id);

                    if (user.Id != id)
                    {
                        throw new UnauthorizedAccessException("Accès refusé");
                    }

                    return businesses;
                } */
    }
}