//using ILR_VALIDATION.Domain.Interfaces;
//using System.Collections.Concurrent;
//using System.Threading.Tasks;

//namespace ILR_VALIDATION.Infrastructure.Services
//{
//    public class MessageQueueService : IMessageQueueService
//    {
//        private static readonly ConcurrentQueue<string> _queue = new();

//        public Task EnqueueAsync(string message)
//        {
//            _queue.Enqueue(message);
//            return Task.CompletedTask;
//        }

//        public bool TryDequeue(out string? item)
//        {
//            return _queue.TryDequeue(out item);
//        }
//    }
//}