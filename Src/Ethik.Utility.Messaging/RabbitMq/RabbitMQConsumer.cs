using Ethik.Utility.Messaging.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQConsumer : IRabbitMQConsumer
{
    private readonly RabbitMQConsumerConfiguration _config;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly IMessageSerializer _serializer;
    private readonly SemaphoreSlim _processingSemaphore;
    private IConnection? _connection;
    private ConcurrentDictionary<int, IChannel> _activeChannels;
    private CancellationTokenSource? _cts;
    private bool _disposed;
    private bool _isStopping = false;
    private readonly List<CancellationTokenSource> _queueCts;
    private readonly Stopwatch autoShutdownWatch;
    private readonly Stopwatch mainWatch;
    private int messageCount;

    public RabbitMQConsumer(RabbitMQConsumerConfiguration config, ILogger<RabbitMQConsumer> logger, IMessageSerializer serializer)
    {
        _activeChannels = new ConcurrentDictionary<int, IChannel>();
        _queueCts = [];
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _processingSemaphore = new SemaphoreSlim(_config.MaxDegreeOfParallelism);
        autoShutdownWatch = Stopwatch.StartNew();
        mainWatch = Stopwatch.StartNew();
        messageCount = 0;
    }

    public async Task StartConsumingAsync()
    {
        if (_cts != null) throw new InvalidOperationException("Consumer already started");

        _cts = new CancellationTokenSource();
        
        await InitializeConnectionAsync();
        StartConsumers();

        if (_config.MaxWaitTimeMilliseconds != -1)
        {
            _ = Task.Run(() => AutoShutdownAsync(_cts.Token))
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                    _logger.LogError(t.Exception, "AutoShutdownAsync failed.");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

    }

    private async Task InitializeConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password,
            VirtualHost = _config.VirtualHost,
            ConsumerDispatchConcurrency = 1, // need more info
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            TopologyRecoveryEnabled = true,
        };

        _connection = await factory.CreateConnectionAsync();
        _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
    }

    private void StartConsumers()
    {
        foreach (var queueName in _config.ListeningQueues)
        {
            var cts = new CancellationTokenSource();
            _queueCts.Add(cts);

            _ = ConsumeQueueWithRestartAsync(queueName, cts.Token);
        }
    }

    public async Task AutoShutdownAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            //_logger.LogInformation(watch.ElapsedMilliseconds.ToString());

            if (autoShutdownWatch.ElapsedMilliseconds > _config.MaxWaitTimeMilliseconds)
            {
                _logger.LogDebug("Waited for {time} ms for messages before shutting down.", _config.MaxWaitTimeMilliseconds);
                await StopConsumingAsync();
                break;
            }

            await Task.Delay(100, token);
        }
    }

    private async Task ConsumeQueueWithRestartAsync(string queueName, CancellationToken appToken)
    {
        while (!appToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeQueueAsync(queueName, appToken);
                break;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                _logger.LogWarning("Queue {Queue} not found, will retry in 30s", queueName);
                await Task.Delay(TimeSpan.FromSeconds(30), appToken);
            }
            catch (Exception ex) when (!appToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Consumer on queue {Queue} failed, restarting in 10s", queueName);
                await Task.Delay(TimeSpan.FromSeconds(10), appToken);
            }
        }
    }

    private async Task ConsumeQueueAsync(string queueName, CancellationToken token)
    {
        if (_connection is null)
        {
            throw new ArgumentNullException(nameof(_connection), "The connection must not be null");
        }
        using var channel = await _connection.CreateChannelAsync();
        _activeChannels[channel.ChannelNumber] = channel;
        channel.ChannelShutdownAsync += OnChannelShutdownAsync;

        _logger.LogInformation("Opening channel {Channel} for queue {Queue}",
                               channel.ChannelNumber, queueName);

        await channel.BasicQosAsync(0, _config.PrefetchCount, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (s, ea) => OnMessageReceivedAsync(queueName, s, ea);

        try
        {
            var consumerTag = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
            _logger.LogInformation("Started consumer {Tag} on channel {Channel} for queue {Queue}",
                                    consumerTag, channel.ChannelNumber, queueName);
        }
        catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
        {
            _logger.LogWarning(ex, "Queue {Queue} not found on channel {Channel}",
                                queueName, channel.ChannelNumber);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consumer for queue {Queue} on channel {Channel}",
                              queueName, channel.ChannelNumber);
            throw;
        }

        _logger.LogInformation("Started consumer for queue {Queue}", queueName);

        var tcs = new TaskCompletionSource<object?>();
        using var reg = token.Register(() => tcs.TrySetCanceled());
        await tcs.Task;
    }

    private Task OnChannelShutdownAsync(object sender, ShutdownEventArgs @event)
    {
        var channel = sender as IChannel;
        if (channel is null)
        {
            throw new ArgumentNullException(nameof(channel), "The channel must not be null");
        }
        _logger.LogWarning("Channel for queue‑consumer shut down: {Code}/{Reason}", @event.ReplyCode, @event.ReplyText);
        var x = _activeChannels.First(s => s.Value.ChannelNumber == channel.ChannelNumber);
        _activeChannels.Remove(x.Key, out var a);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(string queueName, object sender, BasicDeliverEventArgs ea)
    {
        if (_cts is null)
        {
            throw new ArgumentNullException(nameof(_cts), "The cancellation token source must not be null");
        }
        lock (new object())
        {
            autoShutdownWatch.Restart();
        }
        var channel = ((AsyncEventingBasicConsumer)sender).Channel;
        _logger.LogDebug("Received message {Tag} on queue {Queue}", ea.DeliveryTag, queueName);

        await _processingSemaphore.WaitAsync(_cts.Token);
        try
        {
            using var ctsTimeout = new CancellationTokenSource(_config.MessageProcessingTimeout);
            var message = _serializer.Deserialize(ea.Body.ToArray(), _config.MessageType);
            var success = await _config.MessageHandler(
                message,
                new RabbitMQMessageContext {
                    DeliveryTag = ea.DeliveryTag,
                    Headers = ea.BasicProperties?.Headers ?? new Dictionary<string, object?>(),
                    Redelivered = ea.Redelivered,
                    Exchange = ea.Exchange,
                    Queue = queueName,
                },
                ctsTimeout.Token);

            if (success)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                _logger.LogInformation("Acked message {Tag} on queue {Queue}",
                                        ea.DeliveryTag, queueName);
            }
            else
            {
                await RejectAndLogAsync(channel, ea.DeliveryTag, requeue: true, queueName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {Tag} on queue {Queue}",
                             ea.DeliveryTag, queueName);
            await RejectAndLogAsync(channel, ea.DeliveryTag, requeue: true, queueName);
        }
        finally
        {
            _processingSemaphore.Release();
            lock (new object())
            {
                messageCount++;
            }
        }
    }

    private async Task RejectAndLogAsync(IChannel channel, ulong tag, bool requeue, string queueName)
    {
        await channel.BasicNackAsync(tag, false, requeue);
        _logger.LogWarning("Rejected message {Tag} on queue {Queue}. Requeue={Requeue}",
                           tag, queueName, requeue);
    }

    public async Task StopConsumingAsync()
    {
        if (_connection is null)
        {
            throw new ArgumentNullException(nameof(_connection), "The connection must not be null");
        }
        if (_isStopping) return;
        _isStopping = true;

        _logger.LogInformation("Stopping all consumers and closing channels…");
        _cts?.Cancel();

        
        foreach (var kvp in _activeChannels.ToArray())
        {
            var channelNumber = kvp.Key;
            var channel = kvp.Value;

            _logger.LogDebug("Closing channel {Channel}", channelNumber);
            try
            {
                await channel.CloseAsync();       
                _logger.LogInformation("Closed channel {Channel}", channelNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing channel {Channel}", channelNumber);
            }

            try
            {
                channel.Dispose();                
                _logger.LogDebug("Disposed channel {Channel}", channelNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing channel {Channel}", channelNumber);
            }

            _activeChannels.TryRemove(channelNumber, out _);
        }

        try
        {
            await _connection.CloseAsync();
            _logger.LogInformation("Connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing connection");
        }
        finally
        {
            _connection.Dispose();
            var ms = mainWatch.ElapsedMilliseconds;
            mainWatch.Stop();
            _logger.LogDebug("Processed {count} messages & Total time elapsed : {Time}", messageCount, TimeSpan.FromMilliseconds(ms));
        }
    }

    private async Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("Connection shutdown: {ReplyText}", e.ReplyText);
        if (_isStopping || _disposed)
            return;
        await ReconnectAsync();
    }

    private async Task ReconnectAsync()
    {
        var maxRetries = _config.RetryPolicy.MaxRetryAttempts;
        autoShutdownWatch.Stop();
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            _logger.LogInformation("Reconnect attempt {Attempt}/{Max}", attempt, maxRetries);

            foreach (var kvp in _activeChannels.ToArray())
            {
                var channelNumber = kvp.Key;
                var channel = kvp.Value;

                _logger.LogDebug("Reconnecting: closing channel {Channel}", channelNumber);
                try
                {
                    await channel.CloseAsync();
                    _logger.LogInformation("Closed channel {Channel}", channelNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing channel {Channel} during reconnect", channelNumber);
                }

                try
                {
                    channel.Dispose();
                    _logger.LogDebug("Disposed channel {Channel}", channelNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing channel {Channel} during reconnect", channelNumber);
                }

                
                if (_activeChannels.TryRemove(channelNumber, out _))
                    _logger.LogDebug("Removed channel {Channel} from active list", channelNumber);
            }

            
            if (_connection != null)
            {
                _logger.LogDebug("Reconnecting: closing connection");
                try
                {
                    await _connection.CloseAsync();
                    _logger.LogInformation("Connection closed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing connection during reconnect");
                }

                try
                {
                    _connection.Dispose();
                    _logger.LogDebug("Disposed connection");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing connection during reconnect");
                }
            }

            var delay = _config.RetryPolicy.GetRetryDelay(attempt);
            _logger.LogInformation("Waiting {Delay}s before next reconnect attempt", delay);
            await Task.Delay(delay);

            try
            {
                await InitializeConnectionAsync();
                StartConsumers();

                _logger.LogInformation("Reconnected successfully on attempt {Attempt}", attempt);
                autoShutdownWatch.Start();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnect attempt {Attempt} failed", attempt);
            }
        }

        _logger.LogCritical("Failed to reconnect after {MaxRetries} attempts", maxRetries);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            throw new ArgumentNullException(nameof(_connection), "The connection must not be null");
        }
        if (_cts is null)
        {
            throw new ArgumentNullException(nameof(_cts), "The cancellation token source must not be null");
        }
        if (_disposed) return;
        _disposed = true;
        _logger.LogInformation("Stopping RabbitMQConsumer…");

        _cts.Cancel();
        foreach (var ch in _activeChannels.Values)
        {
            _logger.LogDebug("Closing channel {Channel}", ch.ChannelNumber);
            await ch.CloseAsync();
            ch.Dispose();
        }

        await _connection.CloseAsync();
        _connection.Dispose();
        _processingSemaphore.Dispose();
        _cts.Dispose();

        _logger.LogInformation("RabbitMQConsumer stopped.");
    }

    public void Dispose()
    {
        DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}