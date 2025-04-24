using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMqPublisher : IPublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMQPublisherConfiguration _config;
    private IConnection? _connection;
    private SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<int, IChannel> _activeChannels = new ConcurrentDictionary<int, IChannel>();
    private bool _initialized;

    public RabbitMqPublisher(
        RabbitMQPublisherConfiguration config,
        ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _config = config;
    }
    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_initialized) return;

            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                UserName = _config.UserName,
                Password = _config.Password,
                VirtualHost = _config.VirtualHost,
            };

            _connection = await factory.CreateConnectionAsync();
            _initialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    private readonly object _channelLock = new object();

    public async Task PublishAsync<T>(
        T message,
        string topic,
        IDictionary<string, string?>? headers = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();

        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var channel = await GetChannelAsync();

        var props = new BasicProperties();
        props.Persistent = true;

        var formattedHeaders = headers?
            .Where(pair => pair.Value != null)
            .ToDictionary(
                pair => pair.Key,
                pair => (object?)Encoding.UTF8.GetBytes(pair.Value)
            ) ?? new Dictionary<string, object?>();

        if (formattedHeaders != null && !formattedHeaders.ContainsKey("MessageType"))
            formattedHeaders["MessageType"] = (object?)Encoding.UTF8.GetBytes(typeof(T).Name);

        props.Headers = formattedHeaders;

        try
        {
            await channel.BasicPublishAsync(
                exchange: topic,
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken
            );

            _logger.LogDebug("Message published to exchange {Exchange} with type {Type}", topic, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to {Exchange} with type {Type}", topic, typeof(T).Name);
            throw;
        }
    }

    private async Task<IChannel> GetChannelAsync()
    {
        var channelId = Environment.CurrentManagedThreadId;  
        if (!_activeChannels.TryGetValue(channelId, out var channel) || channel.IsClosed)
        {
            _activeChannels[channelId] = await CreateChannelAsync();
            channel = _activeChannels[channelId];
        }

        return channel;
    }

    private async Task<IChannel> CreateChannelAsync()
    {
        _logger.LogDebug("Creating a new channel.");
        return await _connection!.CreateChannelAsync();
    }

    public void Dispose()
    {
        foreach (var channel in _activeChannels.Values)
        {
            channel?.Dispose();
        }

        _connection?.Dispose();
    }
}
