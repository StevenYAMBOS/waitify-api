/* using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace WaitifyApi.Services;

public class BlobService
{
    private const string ContainerName = "users";
    public const string SuccessMessageKey = "SuccessMessage";
    public const string ErrorMessageKey = "ErrorMessage";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        _containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        _containerClient.CreateIfNotExists();
    }

    public BlobServiceClient GetBlobServiceClient(string accountName)
    {
        BlobServiceClient client = new(
            new Uri($"https://{accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());

        return client;
    }

    public async Task GetFiles()
    {
        // Retrieve the connection string for use with the application. 
        string connectionString = Environment.GetEnvironmentVariable("AzureBlobStorage");

        // Create a BlobServiceClient object 
        var blobServiceClient = new BlobServiceClient(connectionString);

        Console.WriteLine("Listing blobs...");

        // List all blobs in the container
        await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync())
        {
            Console.WriteLine("\t" + blobItem.Name);
        }
    }
    private static async Task ListBlobsFlatListing(BlobContainerClient blobContainerClient, int? segmentSize)
    {
        try
        {
            // Call the listing operation and return pages of the specified size.
            var resultSegment = blobContainerClient.GetBlobsAsync().AsPages(default, segmentSize);

            // Enumerate the blobs returned for each page.
            await foreach (Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    Console.WriteLine("Blob name: {0}", blobItem.Name);
                }

                Console.WriteLine();
            }
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
            throw;
        }
    }
} */