using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using WaitifyApi.Constants;

namespace WaitifyApi.Services;

public class FileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<FileStorageService> logger;

    public FileStorageService(BlobServiceClient blobServiceClient, ILogger<FileStorageService> _logger)
    {
        var azureUsersContainer = Environment.GetEnvironmentVariable("AzureBlobUsersContainer");
        _blobServiceClient = blobServiceClient;
        _containerClient = _blobServiceClient.GetBlobContainerClient(azureUsersContainer);
        _containerClient.CreateIfNotExists();
        logger = _logger;
    }

    private string GenerateFileName(string fileName, string ClientName)
    {
        try
        {
            string strFileName = string.Empty;
            string[] strName = fileName.Split('.');
            strFileName = ClientName.Replace(" ", string.Empty) + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd") + "/" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[strName.Length - 1];
            return strFileName;
        }
        catch (Exception ex)
        {
            logger.LogError("Une erreur est survenue lors de la génération du nom du fichier : {@0}", ex);
            return fileName;
        }
    }

    public async Task<string> UploadBlobAsync(IFormFile file, string clientName, string containerName, string[] allowedExtensions)
    {
        ArgumentNullException.ThrowIfNull(file);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"Extensions autorisées : {string.Join(", ", allowedExtensions)}");
        }

        try
        {
            var filename = GenerateFileName(file.FileName, clientName);
            var fileUrl = "";
            string azureConnectionString = Environment.GetEnvironmentVariable("AzureBlobStorage")!;
            var container = new BlobContainerClient(azureConnectionString, containerName);

            string[] extensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];


            BlobClient blobClient = container.GetBlobClient(filename);
            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/webp" };
            using (Stream stream = file.OpenReadStream())
            {
                blobClient.Upload(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
            }
            fileUrl = blobClient.Uri.AbsoluteUri;
            var result = fileUrl;
            logger.LogInformation("Fichier téléchargé avec succès : {@0}", fileUrl);
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Une erreur est survenue lors de l'upload du fichier : ", ex);
        }
    }

    public async Task<string> UpdateExistingBlobAsync(string previousFileName, IFormFile file, string clientName, string containerName, string[] allowedExtensions)
    {
        string azureConnectionString = Environment.GetEnvironmentVariable("AzureBlobStorage")!;
        var container = new BlobContainerClient(azureConnectionString, containerName);

        // On supprime d'abord l'ancien fichier dans le dossier Azure (dossier Azure = nom entreprise + timestamp)
        BlobClient blobClient = container.GetBlobClient(previousFileName);
        logger.LogError("URL ancien dossier Azure `{@0}`.", previousFileName);
        string DEFAULT_LOGO_URL = AppConstants.Azure.WaitifyLogoUrl;
        string DEFAULT_LOGO_URL2 = previousFileName.Replace("Waitify/waitify_logo.png", "");
        if (DEFAULT_LOGO_URL2 == DEFAULT_LOGO_URL)
        {
            logger.LogInformation("Logo de base");
        }
        else
        {
            await blobClient.DeleteAsync(snapshotsOption: DeleteSnapshotsOption.IncludeSnapshots);
        }

        ArgumentNullException.ThrowIfNull(file);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"Extensions autorisées : {string.Join(", ", allowedExtensions)}");
        }

        // On télécharge le nouveau fichier
        try
        {
            var fileUrl = "";

            string[] extensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];
            var filename = GenerateFileName(file.FileName, clientName);
            blobClient = container.GetBlobClient(filename);
            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/webp" };
            using (Stream stream = file.OpenReadStream())
            {
                blobClient.Upload(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
            }
            fileUrl = blobClient.Uri.AbsoluteUri;
            var result = fileUrl;
            logger.LogInformation("Url logo en env : {@0}", DEFAULT_LOGO_URL);
            logger.LogInformation("Split nom logo en BDD : {@0}", DEFAULT_LOGO_URL2);
            logger.LogInformation("fileName : {@0}", filename);
            logger.LogInformation("file.fileName : {@0}", file.FileName);
            logger.LogInformation("Fichier téléchargé avec succès : {@0}", fileUrl);
            return result;
            /*             if (previousFileName is null) // si l'entreprise n'a pas de logo
                        {

                            var filename = GenerateFileName(file.FileName, clientName);
                            var fileUrl = "";

                            string[] extensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];


                            blobClient = container.GetBlobClient(filename);
                            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/webp" };
                            using (Stream stream = file.OpenReadStream())
                            {
                                blobClient.Upload(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
                            }
                            fileUrl = blobClient.Uri.AbsoluteUri;
                            var result = fileUrl;
                            logger.LogInformation("Fichier téléchargé avec succès : {@0}", fileUrl);
                            return result;
                        }
                        else // si l'entreprise avait un logo
                        {
                            var fileUrl = "";

                            string[] extensions = [".jpeg", ".jpg", ".png", ".webp", ".svg"];

                            blobClient = container.GetBlobClient(previousFileName);
                            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/webp" };
                            using (Stream stream = file.OpenReadStream())
                            {
                                blobClient.Upload(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
                            }
                            fileUrl = blobClient.Uri.AbsoluteUri;
                            var result = fileUrl;
                            logger.LogInformation("Fichier téléchargé avec succès : {@0}", fileUrl);
                            return result;
                        } */
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Une erreur est survenue lors de l'upload du fichier : ", ex);
        }
    }

    public async Task DeleteBlobSnapshotsAsync(string fileName, string containerName)
    {
        string azureConnectionString = Environment.GetEnvironmentVariable("AzureBlobStorage")!;
        var container = new BlobContainerClient(azureConnectionString, containerName);
        BlobClient blobClient = container.GetBlobClient(fileName);
        await blobClient.DeleteAsync(snapshotsOption: DeleteSnapshotsOption.IncludeSnapshots);
    }

    /*
    public BlobServiceClient GetBlobServiceClient(string accountName)
    {
        BlobServiceClient client = new(
            new Uri($"https://{accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());

        return client;
    }
*/
}