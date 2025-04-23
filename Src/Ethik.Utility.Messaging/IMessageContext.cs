namespace Ethik.Utility.Messaging;

public interface IMessageContext 
{
    public ulong? DeliveryTag { get; set; }
    public IDictionary<string, string?>? Headers { get; set; }
    public bool? Redelivered { get; set; }
    public string? Exchange { get; set; }
    public string? Queue { get; set; }
}
