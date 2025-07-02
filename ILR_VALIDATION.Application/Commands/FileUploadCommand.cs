using ILR_VALIDATION.Domain.Entities;
using ILR_VALIDATION.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ILR_VALIDATION.Application.Commands
{
    public class UploadFileCommand : IRequest<FileReference>
    {
        public IFormFile? File { get; set; }
    }

    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileReference>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMessageQueueService _messageQueueService;

        public UploadFileCommandHandler(IFileStorageService fileStorageService, IMessageQueueService messageQueueService)
        {
            _fileStorageService = fileStorageService;
            _messageQueueService = messageQueueService;
        }

        public async Task<FileReference> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0) throw new ArgumentException("No file uploaded.");
            var referenceId = Guid.NewGuid().ToString();
            var filePath = Path.Combine("uploads", $"{referenceId}.xml");
            await _fileStorageService.SaveFileAsync(request.File, filePath);
            var fileReference = new FileReference
            {
                ReferenceId = referenceId,
                FileName = request.File.FileName,
                FilePath = filePath
            };
            await _messageQueueService.EnqueueAsync($"{referenceId}.xml");
            return fileReference;
        }
    }
}