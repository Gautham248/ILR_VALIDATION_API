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
        private readonly string _resultPath;

        public ResultGeneratorService(IMessageQueueService messageQueueService, IFileStorageService fileStorageService, IConfiguration configuration)
        {
            _messageQueueService = messageQueueService;
            _fileStorageService = fileStorageService;

            // Fix: Use the GetSection method to retrieve the value manually
            var resultPathSection = configuration.GetSection("Storage:ResultPath");
            _resultPath = resultPathSection.Value ?? "results";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_messageQueueService is MessageQueueService queueService && queueService.TryDequeue(out var referenceId))
                {
                    await Task.Delay(5000); // Simulate processing time
                    var resultPath = Path.Combine(_resultPath, $"{referenceId}.json");
                    var directory = Path.GetDirectoryName(resultPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    var resultContent = "{\"status\": \"processed\", \"data\": \"example\"}";
                    await File.WriteAllTextAsync(resultPath, resultContent, Encoding.UTF8);
                }
                await Task.Delay(1000); // Check every second
            }
        }
    }
}