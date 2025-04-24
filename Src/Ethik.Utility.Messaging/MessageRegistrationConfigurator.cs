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

    public void UseRabbitMQ(IConfiguration configuration, string consumerSectionName = "RabbitMQConfiguration", string? publisherSectionName = null)
    {
        var consumerConfig = new RabbitMQConsumerConfiguration();
        configuration.GetSection(consumerSectionName).Bind(consumerConfig);
        _services.AddSingleton(consumerConfig);

        if (!string.IsNullOrWhiteSpace(publisherSectionName))
        {
            var publisherConfig = new RabbitMQPublisherConfiguration();
            configuration.GetSection(publisherSectionName).Bind(publisherConfig);
            _services.AddSingleton(publisherConfig);
        }
        else
        {
            _services.AddSingleton<RabbitMQPublisherConfiguration>(consumerConfig);
        }

        _services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        _services.AddSingleton<IConsumer, RabbitMQConsumer>();
        _services.AddHostedService<ConsumerHostedService>();
        _services.AddSingleton<IPublisher, RabbitMqPublisher>();
    }

    public void UseRabbitMQ( Action<RabbitMQConsumerConfiguration> rabbitConsumerConfig, Action<RabbitMQPublisherConfiguration>? rabbitPublisherConfig = null)
    {
        var consumerConfig = new RabbitMQConsumerConfiguration();
        rabbitConsumerConfig(consumerConfig);
        _services.AddSingleton(consumerConfig);

        if (rabbitPublisherConfig != null)
        {
            var publisherConfig = new RabbitMQPublisherConfiguration();
            rabbitPublisherConfig(publisherConfig);
            _services.AddSingleton(publisherConfig);
        }
        else
        {
            _services.AddSingleton<RabbitMQPublisherConfiguration>(consumerConfig);
        }

        _services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        _services.AddSingleton<IConsumer, RabbitMQConsumer>();
        _services.AddHostedService<ConsumerHostedService>();
        _services.AddSingleton<IPublisher, RabbitMqPublisher>();
    }
    public void UseRabbitMQ(Action<RabbitMQPublisherConfiguration> rabbitPublisherConfig)
    {
        var publisherConfig = new RabbitMQPublisherConfiguration();
        rabbitPublisherConfig(publisherConfig);
        _services.AddSingleton(publisherConfig);

        _services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        _services.AddSingleton<IPublisher, RabbitMqPublisher>();
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
