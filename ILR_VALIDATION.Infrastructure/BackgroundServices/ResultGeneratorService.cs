using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ILR_VALIDATION.Domain.Interfaces;
using ILR_VALIDATION.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        private readonly ILogger<AzureBlobStorageService> _logger;

        public ResultGeneratorService(ILogger<AzureBlobStorageService> logger,IMessageQueueService messageQueueService, IFileStorageService fileStorageService, IConfiguration configuration)
        {
            _messageQueueService = messageQueueService;
            _fileStorageService = fileStorageService;
            var sasUrl = configuration["Azure:BlobSasUrl"];
            _blobServiceClient = new BlobServiceClient(new Uri(sasUrl));
            _resultContainer = configuration["Azure:ResultContainer"] ?? "ilrfiles";
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var referenceId = await _messageQueueService.DequeueAsync(stoppingToken);
                if (referenceId != null)
                {
                    await Task.Delay(5000); 
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
                    var blobClient = containerClient.GetBlobClient($"{referenceId}.json");

            
                    var validationErrors = GenerateValidationErrors(referenceId);
                    var resultContent = JsonConvert.SerializeObject(new
                    {
                        referenceId,
                        ValidationErrors = validationErrors
                    });

                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultContent));
                    await blobClient.UploadAsync(stream, new BlobUploadOptions());
                }
                await Task.Delay(1000); 
            }
        }

        private List<ValidationError> GenerateValidationErrors(string referenceId)
        {
            var errors = new List<ValidationError>();
            var baseLearnRefNumber = referenceId.Replace("-", "").Substring(0, 6);
            var random = new Random();

            
            string[] learnerNames = { "John Smith", "Aisha Khan", "Raj Mehta", "Emily Brown", "Mohammed Ali",
                                     "Chloe Martin", "David Lee", "Sophia Wilson", "Liam Patel", "Isla Green",
                                     "Ethan Walker", "Olivia Thomas" };
            string[] pmukprns = { "12345678", "87654321" };
            string[] severities = { "Error", "Warning", "Success" };
            string[] messages = {
                  "Date of birth must not be in the future.",
                  "Planned learning hours are unusually high.",
                  "Funding model not valid for the learner's age.",
                  "Missing end date for learning delivery.",
                  "The data is valid."
              };
            string[] ruleIds = { "DOB_01", "PLH_05", "FUND_12", "ENDDATE_02" };

            for (int i = 0; i < 12; i++)
            {
                errors.Add(new ValidationError
                {
                    LearnerName = learnerNames[i],
                    LearnRefNumber = $"{baseLearnRefNumber}{i:000}",
                    PMUKPRN = pmukprns[random.Next(pmukprns.Length)],
                    Severity = severities[random.Next(severities.Length)],
                    Message = messages[random.Next(messages.Length)],
                    RuleID = ruleIds[random.Next(ruleIds.Length)]
                });
            }

            return errors;
        }
    }

    public class ValidationError
    {
        public string LearnerName { get; set; }
        public string LearnRefNumber { get; set; }
        public string PMUKPRN { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string RuleID { get; set; }
    }
}