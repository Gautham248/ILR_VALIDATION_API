using ILR_VALIDATION.Application.Commands;
using ILR_VALIDATION.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ILR_VALIDATION.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FileController> _logger;

        public FileController(IMediator mediator, ILogger<FileController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            _logger.LogInformation("Received file upload request for file: {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid file type uploaded: {FileName}", file.FileName);
                return BadRequest("Only XML files are allowed.");
            }

            var command = new UploadFileCommand { File = file };
            var result = await _mediator.Send(command);
            _logger.LogInformation("File uploaded successfully with ReferenceId: {ReferenceId}", result.ReferenceId);
            return Ok(result);
        }

        [HttpGet("status/{referenceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileStatus(string referenceId)
        {
            _logger.LogInformation("Checking status for ReferenceId: {ReferenceId}", referenceId);
            var query = new StatusCheckQuery { ReferenceId = referenceId };
            var result = await _mediator.Send(query);
            if (result == null)
            {
                _logger.LogWarning("Status check failed for ReferenceId: {ReferenceId}", referenceId);
                return NotFound();
            }
            _logger.LogInformation("Status retrieved for ReferenceId: {ReferenceId}, Status: {Status}", referenceId, result.Status);
            return Ok(result);
        }
    }
}