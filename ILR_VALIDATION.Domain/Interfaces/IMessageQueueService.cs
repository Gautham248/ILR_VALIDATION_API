namespace ILR_VALIDATION.Domain.Interfaces
{
    public interface IMessageQueueService
    {
        Task EnqueueAsync(string message);
        Task<string> DequeueAsync(CancellationToken cancellationToken);
    }
}