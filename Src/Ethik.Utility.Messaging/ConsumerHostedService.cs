using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ethik.Utility.Messaging;

public class ConsumerHostedService : IHostedService
{
    private readonly IConsumer _consumer;
    private readonly ILogger<ConsumerHostedService> _logger;

    public ConsumerHostedService(IConsumer consumer, ILogger<ConsumerHostedService> logger)
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
            _logger.LogError(ex, "Failed to start consumer.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.StopConsumingAsync();
    }
}
