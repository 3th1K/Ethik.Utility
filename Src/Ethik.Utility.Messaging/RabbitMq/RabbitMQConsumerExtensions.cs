using RabbitMQ.Client;

namespace Ethik.Utility.Messaging.RabbitMq;

public static class RabbitMQConsumerExtensions
{
    //public static async Task SelfDeclareExchangeAsync(this IChannel channel, 
    //    string exchangeName, 
    //    ExchangeTypes exchangeType, 
    //    bool overrideExistingDeclaration, 
    //    bool durable=true, 
    //    bool autoDelete=false, 
    //    IDictionary<string, object?>? arguments= null)
    //{
    //    if (overrideExistingDeclaration)
    //    {
    //        await channel.ExchangeDeleteAsync(exchangeName, false);
    //    }
    //    await channel.ExchangeDeclareAsync(
    //            exchange: exchangeName,
    //            type: exchangeType.ToString().ToLower(),
    //            durable: durable,
    //            autoDelete: autoDelete,
    //            arguments: arguments);
    //}
    //public static async Task SelfDeclareQueueAsync(this IChannel channel,
    //    string queueName,
    //    bool overrideExistingDeclaration,
    //    bool durable = true,
    //    bool exclusive=false,
    //    bool autoDelete = false,
    //    IDictionary<string, object?>? arguments = null)
    //{
        
    //    if (overrideExistingDeclaration)
    //    {
    //        await channel.QueueDeleteAsync(queueName, false, false);
    //    }

    //    await channel.QueueDeclareAsync(
    //        queue: queueName,
    //        durable: durable,
    //        exclusive: exclusive,
    //        autoDelete: autoDelete,
    //        arguments: arguments);
    //}
    //public static async Task SelfDeclareBindingAsync(this IChannel channel,
    //    string queueName,
    //    string exchangeName,
    //    string routingKey = "")
    //{
    //    await channel.QueueBindAsync(
    //        queue: queueName,
    //        exchange: exchangeName,
    //        routingKey: routingKey);
    //}
    //public static async Task SelfDeclareDeadLetterAsync(this IChannel channel, 
    //    string deadLetterExchangeName,
    //    string deadLetterQueueName,
    //    bool overrideExistingDeclaration)
    //{
    //    await channel.SelfDeclareExchangeAsync(deadLetterExchangeName, ExchangeTypes.Fanout, overrideExistingDeclaration);
    //    await channel.SelfDeclareQueueAsync(deadLetterQueueName, overrideExistingDeclaration);
    //    await channel.SelfDeclareBindingAsync(deadLetterQueueName, deadLetterExchangeName);
    //}
}
