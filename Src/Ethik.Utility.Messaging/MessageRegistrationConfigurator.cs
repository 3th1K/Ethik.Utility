using Ethik.Utility.Messaging.RabbitMq;
using Ethik.Utility.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ethik.Utility.Messaging;
public class ConsumerRegistrationOptions
{
    public List<Type> ConsumerTypes { get; } = new();
}

public class MessageRegistrationConfigurator
{
    private readonly IServiceCollection _services;
    public List<Type> ConsumerTypes { get; } = [];

    public MessageRegistrationConfigurator(IServiceCollection services)
    {
        _services = services;
    }

    public void UseRabbitMQ(IConfiguration configuration, string sectionName = "RabbitMQConfiguration")
    {
        var config = new RabbitMQConsumerConfiguration();
        configuration.GetSection(sectionName).Bind(config);

        _services.AddSingleton(config);
        _services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        _services.AddSingleton<RabbitMQConsumer>();
        _services.AddHostedService<RabbitMQConsumerHostedService>();
    }

    public void UseRabbitMQ(Action<RabbitMQConsumerConfiguration> rabbitConfig)
    {
        var config = new RabbitMQConsumerConfiguration();
        rabbitConfig(config);

        _services.AddSingleton(config);
        _services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        _services.AddSingleton<RabbitMQConsumer>();
        _services.AddHostedService<RabbitMQConsumerHostedService>();
    }

    public void AddConsumer<TConsumer>() where TConsumer : class
    {
        var interfaceType = typeof(TConsumer).GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>));

        if (interfaceType == null)
        {
            throw new InvalidOperationException($"{typeof(TConsumer).Name} does not implement IMessageConsumer<T>.");
        }

        _services.AddSingleton(typeof(TConsumer));

        ConsumerTypes.Add(typeof(TConsumer));
    }

    public void AddConsumersFromAssembly(Assembly assembly)
    {
        var consumerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>)))
            .ToList();

        foreach (var consumerType in consumerTypes)
        {
            _services.AddSingleton(consumerType);
            ConsumerTypes.Add(consumerType);
        }
    }

    public void AddConsumersFromAssemblyContaining<T>()
    {
        AddConsumersFromAssembly(typeof(T).Assembly);
    }

    public void AddConsumersFromAssemblies(params Assembly[] assemblies)
    {
        var consumerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageConsumer<>)))
            .ToList();

        foreach (var consumerType in consumerTypes)
        {
            _services.AddSingleton(consumerType);
            ConsumerTypes.Add(consumerType);
        }

    }
}
