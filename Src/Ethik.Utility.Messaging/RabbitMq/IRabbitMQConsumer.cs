

namespace Ethik.Utility.Messaging.RabbitMq;

public interface IRabbitMQConsumer : IAsyncDisposable
{
    Task StartConsumingAsync();
    Task StopConsumingAsync();
}
