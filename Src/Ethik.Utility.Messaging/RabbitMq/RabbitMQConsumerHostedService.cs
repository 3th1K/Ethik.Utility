using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQConsumerHostedService : IHostedService
{
    private readonly RabbitMQConsumer _consumer;
    private readonly ILogger<RabbitMQConsumerHostedService> _logger;

    public RabbitMQConsumerHostedService(RabbitMQConsumer consumer, ILogger<RabbitMQConsumerHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _consumer.StartConsumingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumer.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.StopConsumingAsync();
    }
}
