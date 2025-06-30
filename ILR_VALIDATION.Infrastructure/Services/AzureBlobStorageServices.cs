using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ILR_VALIDATION.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _uploadContainer;
        private readonly string _resultContainer;

        private readonly ILogger<AzureBlobStorageService> _logger;
        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;
            var sasUrl = configuration["Azure:BlobSasUrl"];
            _logger.LogInformation("Initializing with SAS URL: {SasUrl}", sasUrl);
            _blobServiceClient = new BlobServiceClient(new Uri(sasUrl));
            _uploadContainer = configuration["Azure:UploadContainer"] ?? "ilrfiles";
            _resultContainer = configuration["Azure:ResultContainer"] ?? "ilrfiles";
        }

        public async Task SaveFileAsync(IFormFile file, string filePath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_uploadContainer);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions());
        }

        public async Task<string?> ReadFileAsync(string filePath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }
            return null;
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));
            return blobClient.ExistsAsync().ContinueWith(task => task.Result.Value);
        }
    }
}