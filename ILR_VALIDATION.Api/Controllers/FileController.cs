using ILR_VALIDATION.Application.Commands;
using ILR_VALIDATION.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileController> _logger;

        public FileController(IMediator mediator, IFileStorageService fileStorageService, ILogger<FileController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload attempt with null or empty file.");
                return BadRequest("No file uploaded.");
            }

            try
            {
                var command = new UploadFileCommand { File = file };
                var result = await _mediator.Send(command);
                _logger.LogInformation("File uploaded successfully for referenceId: {ReferenceId}", result?.ReferenceId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");
                return StatusCode(500, "An error occurred while uploading the file. Check server logs for details.");
            }
        }

        [HttpGet("status/{referenceId}")]
        public async Task<IActionResult> GetStatus(string referenceId)
        {
            if (string.IsNullOrEmpty(referenceId))
            {
                _logger.LogWarning("GetStatus called with null or empty referenceId.");
                return BadRequest("ReferenceId is required.");
            }

            var filePath = $"{referenceId}.json";
            var status = "InProgress";

            try
            {
                _logger.LogInformation("Starting status check for referenceId: {ReferenceId}, FilePath: {FilePath}", referenceId, filePath);
                bool fileExists = await _fileStorageService.FileExistsAsync(filePath);
                _logger.LogInformation("File existence check result for {FilePath}: {Exists}, Container: {_resultContainer}", filePath, fileExists, _fileStorageService.GetType().GetProperty("ContainerName")?.GetValue(_fileStorageService));

                if (fileExists)
                {
                    status = "Completed";
                    var jsonContent = await _fileStorageService.ReadFileAsync(filePath);
                    _logger.LogInformation("Read JSON content for {FilePath}: {Content}", filePath, jsonContent ?? "null");

                    if (jsonContent != null)
                    {
                        _logger.LogInformation("Returning JSON content for referenceId: {ReferenceId}", referenceId);
                        return Content(jsonContent, "application/json");
                    }
                    else
                    {
                        _logger.LogWarning("JSON content is null for referenceId: {ReferenceId}", referenceId);
                    }
                }
                else
                {
                    _logger.LogInformation("No file found for referenceId: {ReferenceId}", referenceId);
                }

                return Ok(new
                {
                    referenceId,
                    fileName = (string?)null,
                    filePath = (string?)null,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for referenceId: {ReferenceId}", referenceId);
                return StatusCode(500, "An error occurred while checking the status. Check server logs for details.");
            }
        }
    }
}