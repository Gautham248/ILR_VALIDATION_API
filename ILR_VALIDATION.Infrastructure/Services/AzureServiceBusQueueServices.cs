using Azure.Messaging.ServiceBus;
using ILR_VALIDATION.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILR_VALIDATION.Infrastructure.Services
{
    public class AzureServiceBusQueueService : IMessageQueueService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusReceiver _receiver;

        public AzureServiceBusQueueService(IConfiguration configuration)
        {
            // Fixed the error by using GetSection and Value property  
            var connectionString = configuration.GetSection("Azure:ServiceBusConnection").Value;
            _serviceBusClient = new ServiceBusClient(connectionString);
            _sender = _serviceBusClient.CreateSender("ilr-xml-process-queue");
            _receiver = _serviceBusClient.CreateReceiver("ilr-xml-process-queue", new ServiceBusReceiverOptions());
        }

        public async Task EnqueueAsync(string message)
        {
            await _sender.SendMessageAsync(new ServiceBusMessage(message));
        }

        public async Task<string> DequeueAsync(CancellationToken cancellationToken)
        {
            var message = await _receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
            if (message != null)
            {
                await _receiver.CompleteMessageAsync(message);
                return message.Body.ToString();
            }
            return null;
        }
    }
}
