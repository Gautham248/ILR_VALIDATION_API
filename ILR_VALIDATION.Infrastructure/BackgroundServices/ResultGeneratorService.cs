//using Azure.Storage.Blobs;
//using Azure.Storage.Blobs.Models;
//using ILR_VALIDATION.Domain.Interfaces;
//using ILR_VALIDATION.Infrastructure.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ILR_VALIDATION.Infrastructure.BackgroundServices
//{
//    public class ResultGeneratorService : BackgroundService
//    {
//        private readonly IMessageQueueService _messageQueueService;
//        private readonly IFileStorageService _fileStorageService;
//        private readonly BlobServiceClient _blobServiceClient;
//        private readonly string _resultContainer;
//        private readonly ILogger<ResultGeneratorService> _logger; // Corrected logger type

//        public ResultGeneratorService(ILogger<ResultGeneratorService> logger, IMessageQueueService messageQueueService, IFileStorageService fileStorageService, IConfiguration configuration)
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
//            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
//            var sasUrl = configuration["Azure:BlobSasUrl"];
//            _blobServiceClient = new BlobServiceClient(new Uri(sasUrl)) ?? throw new ArgumentNullException(nameof(sasUrl));
//            _resultContainer = configuration["Azure:ResultContainer"] ?? "ilrfiles";
//            _logger.LogInformation("Initializing ResultGeneratorService with SAS URL: {SasUrl}, ResultContainer: {ResultContainer}", sasUrl, _resultContainer);
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("ResultGeneratorService started, waiting for messages...");
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                var referenceId = await _messageQueueService.DequeueAsync(stoppingToken);
//                if (referenceId != null)
//                {
//                    _logger.LogInformation("Processing referenceId: {ReferenceId}", referenceId);
//                    await Task.Delay(5000); // Simulate processing time
//                    var containerClient = _blobServiceClient.GetBlobContainerClient(_resultContainer);
//                    var blobClient = containerClient.GetBlobClient($"{referenceId}.json");

//                    try
//                    {
//                        var resultData = GenerateResultData(referenceId);
//                        var resultContent = JsonConvert.SerializeObject(resultData);
//                        _logger.LogInformation("Generated result content: {Content}", resultContent);

//                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultContent));
//                        await blobClient.UploadAsync(stream, new BlobUploadOptions());
//                        _logger.LogInformation("Result uploaded successfully for referenceId: {ReferenceId}", referenceId);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Failed to generate or upload result for referenceId: {ReferenceId}", referenceId);
//                        throw;
//                    }
//                }
//                await Task.Delay(1000); // Check every second
//            }
//        }

//        private object GenerateResultData(string referenceId)
//        {
//            var results = new List<ResultItem>();
//            var random = new Random();
//            int id = 1947; // Static ID as per sample, can be made dynamic if needed

//            string[] learnerNames = { "Jane Smith", "Louise Jones", "Mark Jones", "Smith John", "Jane Smith", "Parker Steve" };
//            string[] learnerRefNumbers = { "18Learner", "CLLearner", "OtherAdult", "Levy", "TLevelT", "SBLearnerEV" };
//            string[] severities = { "Error", "Warning" };
//            string[] messages = {
//                  "GCSE maths qualification grade must be 'NONE'",
//                  "This Unique learner number should not be used",
//                  "The Unique learner number does not pass the checksum calculation",
//                  "The Planned learning hours have not been returned",
//                  "The GCSE maths qualification grade has not been returned",
//                  "The Primary LLDD and health problem is not recorded on one of the LLDD and health problem records"
//              };
//            string[] ruleIds = { "MathGrade_04", "ULN_02", "ULN_04", "PlanLearnHours_01", "MathGrade_01", "PrimaryLLDD_01" };

//            for (int i = 0; i < learnerNames.Length; i++)
//            {
//                results.Add(new ResultItem
//                {
//                    LearnerName = learnerNames[i],
//                    LearnerRefNumber = learnerRefNumbers[i],
//                    PMUKPRN = 0, // Static value as per sample
//                    Severity = severities[random.Next(severities.Length)],
//                    Message = messages[i],
//                    RuleId = ruleIds[i]
//                });
//            }

//            return new
//            {
//                Result = results,
//                Id = id,
//                Exception = (string)null,
//                Status = 5,
//                IsCanceled = false,
//                IsCompleted = true,
//                IsCompletedSuccessfully = true,
//                CreationOptions = 0,
//                AsyncState = (string)null,
//                IsFaulted = false
//            };
//        }
//    }

//    public class ResultItem
//    {
//        public string LearnerName { get; set; }
//        public string LearnerRefNumber { get; set; }
//        public int PMUKPRN { get; set; }
//        public string Severity { get; set; }
//        public string Message { get; set; }
//        public string RuleId { get; set; }
//    }
//}