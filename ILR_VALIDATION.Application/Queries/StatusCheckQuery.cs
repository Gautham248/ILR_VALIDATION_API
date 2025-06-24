using ILR_VALIDATION.Domain.Entities;
using ILR_VALIDATION.Domain.Interfaces;
using MediatR;

namespace ILR_VALIDATION.Application.Queries
{
    public class StatusCheckQuery : IRequest<FileReference>
    {
        public string? ReferenceId { get; set; }
    }

    public class StatusCheckQueryHandler : IRequestHandler<StatusCheckQuery, FileReference>
    {
        private readonly IFileStorageService _fileStorageService;

        public StatusCheckQueryHandler(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<FileReference> Handle(StatusCheckQuery request, CancellationToken cancellationToken)
        {
            var resultPath = Path.Combine("results", $"{request.ReferenceId}.json");
            if (await _fileStorageService.FileExistsAsync(resultPath))
            {
                var content = await _fileStorageService.ReadFileAsync(resultPath);
                return new FileReference
                {
                    ReferenceId = request.ReferenceId,
                    Status = "Completed",
                    ResultContent = content
                };
            }
            return new FileReference
            {
                ReferenceId = request.ReferenceId,
                Status = "Pending"
            };
        }
    }
}