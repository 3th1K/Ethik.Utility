using Ethik.Utility.Messaging.RabbitMq;

namespace Ethik.Utility.Messaging;

public interface IMessageConsumer<in TMessage> where TMessage : class
{
    Task<bool> ConsumeAsync(TMessage message, IMessageContext context, CancellationToken token);
}
