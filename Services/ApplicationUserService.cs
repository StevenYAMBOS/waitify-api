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

            /*
                  var articles = await context.Articles
                    .Where(a => a.AuthorId == id)
                    .ToListAsync();

                  foreach (var article in articles)
                  {
                    if (article.Cover != null)
                      await fileService.DeleteFileAsync(article.Cover.Replace("https://pub-56d2c024e16e477e9fe29e4b168d78ec.r2.dev/", ""));
                  }
            */

            if (user.ProfilePicture != null)
            {
                string blobUrl = Environment.GetEnvironmentVariable("AzureGenericBlobsUrl")!;
                string blobFileName = user.ProfilePicture.Replace(blobUrl, "");
                string azureContainerName = Environment.GetEnvironmentVariable("AzureBlobUsersContainer")!;
                await fileStorageService.DeleteBlobSnapshotsAsync(blobFileName, azureContainerName);
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}