using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ILR_VALIDATION.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _uploadContainer;
        private readonly string _resultContainer;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            // Use GetSection and Value property instead of GetValue
            var connectionString = configuration.GetSection("Azure:BlobConnectionString").Value;
            _blobServiceClient = new BlobServiceClient(connectionString);
            _uploadContainer = configuration.GetSection("Azure:UploadContainer").Value ?? "uploads";
            _resultContainer = configuration.GetSection("Azure:ResultContainer").Value ?? "results";
            InitializeContainers();
        }

        private void InitializeContainers()
        {
            _blobServiceClient.GetBlobContainerClient(_uploadContainer).CreateIfNotExists();
            _blobServiceClient.GetBlobContainerClient(_resultContainer).CreateIfNotExists();
        }

        public async Task SaveFileAsync(IFormFile file, string filePath)
        {
            var container = _blobServiceClient.GetBlobContainerClient(filePath.StartsWith("uploads") ? _uploadContainer : _resultContainer);
            var blobClient = container.GetBlobClient(Path.GetFileName(filePath));
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions());
        }

        public async Task<string?> ReadFileAsync(string filePath)
        {
            var container = _blobServiceClient.GetBlobContainerClient(filePath.StartsWith("results") ? _resultContainer : _uploadContainer);
            var blobClient = container.GetBlobClient(Path.GetFileName(filePath));
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }
            return null;
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var container = _blobServiceClient.GetBlobContainerClient(filePath.StartsWith("results") ? _resultContainer : _uploadContainer);
            var blobClient = container.GetBlobClient(Path.GetFileName(filePath));
            return blobClient.ExistsAsync().ContinueWith(task => task.Result.Value);
        }
    }
}
