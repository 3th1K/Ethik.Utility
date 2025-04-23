using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Ethik.Utility.Messaging;
public static class ConsumerExtensions
{
    public static IServiceCollection UseMessaging(this IServiceCollection services, Action<MessageRegistrationConfigurator> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var configurator = new MessageRegistrationConfigurator(services);
        configure(configurator);

        services.AddSingleton(provider =>
        new ConsumerExecutorRegistry(provider, configurator.ConsumerTypes));

        return services;
    }
}
