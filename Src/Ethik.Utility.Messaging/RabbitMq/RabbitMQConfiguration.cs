namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQConsumerConfiguration : RabbitMQPublisherConfiguration
{
    public ICollection<string> ListeningQueues { get; set; } = [];
    public ushort PrefetchCount { get; set; } = 100;
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount * 2;
    public TimeSpan MessageProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public RetryPolicy RetryPolicy { get; set; } = new RetryPolicy();
    public TimeSpan? MaxWaitTimeMilliseconds { get; set; } = null;
    public ushort NumberOfWorkers { get; set; } = 1;
}
public class RabbitMQPublisherConfiguration
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}
