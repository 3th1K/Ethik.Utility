

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQMessageContext : IMessageContext
{
    public ulong? DeliveryTag { get; set; }
    public IDictionary<string, string>? Headers { get; set; } = null!;
    public bool? Redelivered { get; set; }
    public string? Exchange { get; set; } = null!;
    public string? Queue { get; set; } = null!;
}