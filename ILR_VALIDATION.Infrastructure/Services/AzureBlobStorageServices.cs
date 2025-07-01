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
            _logger.LogInformation("Reading file from path: {FilePath}, Container: {_resultContainer}", filePath, _resultContainer);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));
            try
            {
                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadContentAsync();
                    var content = response.Value.Content.ToString();
                    _logger.LogInformation("File read successfully: {FilePath}, Content: {Content}", filePath, content);
                    return content;
                }
                _logger.LogWarning("File not found during read: {FilePath}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read file: {FilePath} in {Container}", filePath, _resultContainer);
                return null;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            _logger.LogInformation("Checking existence of file: {FilePath}, Container: {_resultContainer}", filePath, _resultContainer);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));
            try
            {
                var response = await blobClient.ExistsAsync();
                _logger.LogInformation("Existence check result for {BlobName} in {Container}: {Exists}, Status: {Status}", blobClient.Name, _resultContainer, response.Value, response.GetRawResponse().Status);
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check existence of file: {FilePath} in {Container}", filePath, _resultContainer);
                return false;
            }
        }
    }
}