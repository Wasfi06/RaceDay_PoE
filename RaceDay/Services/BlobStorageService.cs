using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration[
                "AzureBlobStorage:ConnectionString"];

            _containerName = configuration[
                "AzureBlobStorage:ContainerName"]
                ?? "profile-images";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage connection string is not configured.");
            }

            _blobServiceClient =
                new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadProfileImageAsync(
            Stream imageStream,
            string fileName,
            string contentType)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            var blobName =
                $"{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(
                imageStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            return blobClient.Uri.ToString();
        }
    }
}
