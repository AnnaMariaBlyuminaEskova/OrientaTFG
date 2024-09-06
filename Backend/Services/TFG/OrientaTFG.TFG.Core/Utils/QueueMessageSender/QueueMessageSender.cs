using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrientaTFG.TFG.Core.Utils.QueueMessageSender;

public class QueueMessageSender : IQueueMessageSender
{
    private readonly string _connectionString;

    public QueueMessageSender(IConfiguration configuration)
    {
        _connectionString = configuration["ServiceBusConnectionString"];
    }

    public async Task SendMessageToQueueAsync<T>(T message, string queueName)
    {
        await using (ServiceBusClient client = new ServiceBusClient(_connectionString))
        {
            ServiceBusSender sender = client.CreateSender(queueName);

            // Serialize the message to JSON
            string messageBody = JsonSerializer.Serialize(message);
            ServiceBusMessage busMessage = new ServiceBusMessage(messageBody);

            // Send the message to the queue
            await sender.SendMessageAsync(busMessage);
        }
    }
}

