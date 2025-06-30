using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ILR_VALIDATION.Domain.Interfaces;
using ILR_VALIDATION.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Infrastructure.BackgroundServices
{
    public class ResultGeneratorService : BackgroundService
    {
        private readonly IMessageQueueService _messageQueueService;
        private readonly IFileStorageService _fileStorageService;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _resultContainer;

        public ResultGeneratorService(IMessageQueueService messageQueueService, IFileStorageService fileStorageService, IConfiguration configuration)
        {
            _messageQueueService = messageQueueService;
            _fileStorageService = fileStorageService;
            var sasUrl = configuration["Azure:BlobSasUrl"];
            _blobServiceClient = new BlobServiceClient(new Uri(sasUrl));
            _resultContainer = configuration["Azure:ResultContainer"] ?? "ilrfiles";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var referenceId = await _messageQueueService.DequeueAsync(stoppingToken);
                if (referenceId != null)
                {
                    await Task.Delay(5000); // Simulate processing time
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
                    var blobClient = containerClient.GetBlobClient($"{referenceId}.json");
                    var resultContent = "{\"status\": \"processed\", \"data\": \"example\"}";
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultContent));
                    await blobClient.UploadAsync(stream, new BlobUploadOptions());
                }
                await Task.Delay(1000); // Check every second
            }
        }
    }
}