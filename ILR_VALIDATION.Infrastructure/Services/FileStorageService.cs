using ILR_VALIDATION.Domain.Interfaces;
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Configuration;
  using System.IO;
  using System.Threading.Tasks;

  namespace ILR_VALIDATION.Infrastructure.Services
  {
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;
        private readonly string _resultPath;

        public FileStorageService(IConfiguration configuration)
        {
            // Fixed CS1061 by replacing GetValue with GetSection and Value property
            _uploadPath = configuration.GetSection("Storage:UploadPath").Value ?? "uploads";
            _resultPath = configuration.GetSection("Storage:ResultPath").Value ?? "results";
        }

        public async Task SaveFileAsync(IFormFile file, string filePath)
        {
            var fullPath = Path.Combine(filePath.StartsWith("uploads") ? _uploadPath : _resultPath, Path.GetFileName(filePath));
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task<string?> ReadFileAsync(string filePath)
        {
            var fullPath = Path.Combine(filePath.StartsWith("results") ? _resultPath : _uploadPath, Path.GetFileName(filePath));
            if (File.Exists(fullPath))
                return await File.ReadAllTextAsync(fullPath);
            return null;
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(filePath.StartsWith("results") ? _resultPath : _uploadPath, Path.GetFileName(filePath));
            return Task.FromResult(File.Exists(fullPath));
        }
    }
  }