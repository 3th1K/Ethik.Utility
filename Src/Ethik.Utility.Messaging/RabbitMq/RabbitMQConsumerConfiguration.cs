namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQConsumerConfiguration
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
    public ICollection<string> ListeningQueues { get; set; } = [];
    public ushort PrefetchCount { get; set; } = 100;
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount * 2;
    public TimeSpan MessageProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public RetryPolicy RetryPolicy { get; set; } = new RetryPolicy();
    //public Type MessageType { get; set; } = null!;
    //public Func<object, RabbitMQMessageContext, CancellationToken, Task<bool>> MessageHandler { get; set; } = null!;
    public TimeSpan? MaxWaitTimeMilliseconds { get; set; } = null;
    public ushort NumberOfWorkers { get; set; } = 1;
}
