namespace OrientaTFG.TFG.Core.Utils.QueueMessageSender;

public interface IQueueMessageSender
{
    Task SendMessageToQueueAsync<T>(T message, string queueName);
}
