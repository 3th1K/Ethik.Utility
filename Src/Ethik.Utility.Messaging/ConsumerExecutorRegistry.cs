using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace Ethik.Utility.Messaging;

public class ConsumerExecutorRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, Func<object, IMessageContext, CancellationToken, Task<bool>>> _handlers = new();
    private readonly HashSet<Type> _registeredConsumers = new();

    public ConsumerExecutorRegistry(IServiceProvider serviceProvider, IEnumerable<Type> consumerTypes)
    {
        _serviceProvider = serviceProvider;
        foreach(var consumerType in consumerTypes)
        {
            if (!_registeredConsumers.Add(consumerType)) return;

            var interfaceType = consumerType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>));

            if (interfaceType == null) return;

            var messageType = interfaceType.GetGenericArguments()[0];

            var method = typeof(ConsumerExecutorRegistry)
                .GetMethod(nameof(CreateExecutor), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(messageType, consumerType);

            var executor = (Func<object, IMessageContext, CancellationToken, Task<bool>>)method.Invoke(this, null)!;
            _handlers[messageType] = executor;
        }
    }

    private Func<object, IMessageContext, CancellationToken, Task<bool>> CreateExecutor<TMessage, TConsumer>()
        where TConsumer : IMessageConsumer<TMessage>
        where TMessage : class
    {
        return async (msg, ctx, token) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<TConsumer>();
            return await consumer.ConsumeAsync((TMessage)msg, ctx, token);
        };
    }

    public bool TryGetExecutor(Type messageType, out Func<object, IMessageContext, CancellationToken, Task<bool>>? executor)
    {
        var result = _handlers.TryGetValue(messageType, out executor);
        return result;
    }
}
