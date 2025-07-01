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

            var filePath = $"ilrfiles/{referenceId}.json";
            var status = "InProgress";

            try
            {
                if (await _fileStorageService.FileExistsAsync(filePath))
                {
                    status = "Completed";
                    var jsonContent = await _fileStorageService.ReadFileAsync(filePath);
                    if (jsonContent != null)
                    {
                        _logger.LogInformation("Status check successful for referenceId: {ReferenceId}", referenceId);
                        return Content(jsonContent, "application/json");
                    }
                    _logger.LogWarning("JSON content is null for referenceId: {ReferenceId}", referenceId);
                }
                else
                {
                    _logger.LogInformation("Status is InProgress for referenceId: {ReferenceId}", referenceId);
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