using ILR_VALIDATION.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ILR_VALIDATION.Application.Interfaces
{
    public interface IApplicationService
    {
        Task<FileReference> UploadFileAsync(IFormFile file);
        Task<FileReference> GetFileStatusAsync(string referenceId);
    }
}