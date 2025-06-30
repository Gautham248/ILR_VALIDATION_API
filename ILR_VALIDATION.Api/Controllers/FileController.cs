using ILR_VALIDATION.Application.Commands;
using ILR_VALIDATION.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public FileController(IMediator mediator, IFileStorageService fileStorageService)
        {
            _mediator = mediator;
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var command = new UploadFileCommand { File = file };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("status/{referenceId}")]
        public async Task<IActionResult> GetStatus(string referenceId)
        {
            var filePath = $"ilrfiles/{referenceId}.json"; // Path to the JSON result file
            var status = "InProgress"; // Default status

            if (await _fileStorageService.FileExistsAsync(filePath))
            {
                status = "Completed";
                var jsonContent = await _fileStorageService.ReadFileAsync(filePath);
                if (jsonContent != null)
                {
                    return Content(jsonContent, "application/json");
                }
            }

            return Ok(new
            {
                referenceId,
                fileName = (string?)null,
                filePath = (string?)null,
                status
            });
        }
    }
}