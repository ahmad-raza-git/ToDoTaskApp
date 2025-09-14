using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace TodoApp.Services
{
    public class BlobService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobService(IConfiguration config)
        {
            var connectionString = config["AzureBlobStorage:ConnectionString"];
            var containerName = config["AzureBlobStorage:ContainerName"] ?? "files";
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists(); // ok for dev; ensure container exists in prod
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            // make filename unique to avoid collisions
            var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var blobClient = _containerClient.GetBlobClient(uniqueName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString(); // returns blob URL (if container is public) — or you'll use download via server
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            var download = await blobClient.DownloadAsync();
            return download.Value.Content;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
