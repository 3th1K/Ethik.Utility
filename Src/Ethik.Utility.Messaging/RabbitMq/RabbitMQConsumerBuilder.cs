using Ethik.Utility.Messaging.Serialization;
using Microsoft.Extensions.Logging;

namespace Ethik.Utility.Messaging.RabbitMq;
public class RabbitMQConsumerBuilder
{
    private readonly RabbitMQConsumerConfiguration _config = new();

    public RabbitMQConsumerBuilder SetConnection(string host, int port, string user, string password)
    {
        _config.HostName = host;
        _config.Port = port;
        _config.UserName = user;
        _config.Password = password;
        return this;
    }

    public RabbitMQConsumerBuilder ListeningQueues(ICollection<string> listeningQueues)
    {
        _config.ListeningQueues = listeningQueues;
        return this;
    }

    public RabbitMQConsumerBuilder SetRetryPolicy(Action<RetryPolicy> configure)
    {
        configure(_config.RetryPolicy ??= new RetryPolicy());
        return this;
    }

    public RabbitMQConsumerBuilder MessageHandler<T>(Func<T, RabbitMQMessageContext, CancellationToken, Task<bool>> handler)
    {
        _config.MessageType = typeof(T);
        _config.MessageHandler = async (msg, ctx, ct) =>
            await handler((T)msg, ctx, ct);
        return this;
    }

    public RabbitMQConsumerBuilder WithConfiguration(Action<RabbitMQConsumerConfiguration> configure)
    {
        configure(_config);
        return this;
    }

    public RabbitMQConsumer Build(ILogger<RabbitMQConsumer> logger, IMessageSerializer serializer)
    {
        ValidateConfiguration();
        SetDefaultRetryPolicyIfMissing();
        return new RabbitMQConsumer(_config, logger, serializer);
    }

    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(_config.HostName))
            errors.Add("Host name must be specified");

        if (string.IsNullOrEmpty(_config.UserName))
            errors.Add("Username name must be specified");

        if (string.IsNullOrEmpty(_config.Password))
            errors.Add("Password name must be specified");

        if (!_config.ListeningQueues.Any())
            errors.Add("Listening queues must be specified");

        if (_config.MessageHandler == null)
            errors.Add("Message handler must be specified");

        if (_config.MessageType == null)
            errors.Add("Message type must be specified through WithMessageHandler");

        if (_config.MaxWaitTimeMilliseconds != -1 && _config.MaxWaitTimeMilliseconds < 500)
            errors.Add("Maximum wait time for messages cannot be less than 500 milliseconds");

        if (errors.Count > 0)
            throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", errors)}");
    }

    private void SetDefaultRetryPolicyIfMissing()
    {
        if (_config.RetryPolicy == null)
        {
            _config.RetryPolicy = new RetryPolicy
            {
                MaxRetryAttempts = 3,
                InitialDelay = TimeSpan.FromSeconds(1),
                BackoffExponent = 2
            };
        }
    }

    public RabbitMQConsumerBuilder SetParallelism(int maxDegreeOfParallelism)
    {
        _config.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        return this;
    }

    public RabbitMQConsumerBuilder SetPrefetchCount(ushort prefetchCount)
    {
        _config.PrefetchCount = prefetchCount;
        return this;
    }

    public RabbitMQConsumerBuilder SetProcessingTimeout(TimeSpan timeout)
    {
        _config.MessageProcessingTimeout = timeout;
        return this;
    }

    public RabbitMQConsumerBuilder SetMaxWaitTimeForMessages(long milliseconds)
    {
        _config.MaxWaitTimeMilliseconds = milliseconds;
        return this;
    }
}