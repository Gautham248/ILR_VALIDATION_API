namespace ILR_VALIDATION.Domain.Entities
{
    public class FileReference
    {
        public string? ReferenceId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public string? ResultContent { get; set; }
    }
}