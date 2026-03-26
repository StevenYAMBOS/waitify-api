using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
            strFileName = ClientName + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd") + "/" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[strName.Length - 1];
            return strFileName;
        }
        catch (Exception ex)
        {
            logger.LogInformation("Une erreur est survenue lors de la génération du nom du fichier.", ex);
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