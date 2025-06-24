using Microsoft.AspNetCore.Http;

namespace ILR_VALIDATION.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task SaveFileAsync(IFormFile file, string filePath);
        Task<string> ReadFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
    }
}