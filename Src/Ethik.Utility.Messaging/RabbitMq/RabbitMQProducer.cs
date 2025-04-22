using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQProducer
{
    private readonly ILogger<RabbitMQProducer> _logger;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMQProducer(ILogger<RabbitMQProducer> logger, ConnectionFactory? factory = null)
    {
        _logger = logger;
        _connectionFactory = factory ?? new ConnectionFactory { HostName = "localhost" };
    }

    public async Task SendMessagesToExchangesAsync<T>(T message, string[] exchanges, int messagesPerExchange = 50000, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var tasks = exchanges.Select(exchange =>
                SendMessagesAsync<T>(connection, message, exchange, messagesPerExchange, cancellationToken));

            await Task.WhenAll(tasks);
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError(ex, "Failed to connect to the RabbitMQ broker.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while sending messages.");
            throw;
        }
    }

    private async Task SendMessagesAsync<T>(IConnection connection, T message, string exchange, int count, CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync();

        for (int i = 1; i <= count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var body = new JsonMessageSerializer().Serialize(message);

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: "",
                body: body, 
                cancellationToken);

            if (i % 1000 == 0)
            {
                _logger.LogInformation("Sent {Count} messages to {Exchange}", i, exchange);
            }
        }

        _logger.LogInformation("Completed sending {Total} messages to {Exchange}", count, exchange);
    }
}