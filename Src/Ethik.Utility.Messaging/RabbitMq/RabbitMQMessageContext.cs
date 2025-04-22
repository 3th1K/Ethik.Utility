

namespace Ethik.Utility.Messaging.RabbitMq;

public class RabbitMQMessageContext
{
    public ulong DeliveryTag { get; set; }
    public IDictionary<string, object?> Headers { get; set; } = null!;
    public bool Redelivered { get; set; }
    public string Exchange { get; set; } = null!;
    public string Queue { get; set; } = null!;
}
