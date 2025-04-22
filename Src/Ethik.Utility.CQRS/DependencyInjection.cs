
using Microsoft.Extensions.DependencyInjection;

namespace Ethik.Utility.CQRS;

public static class DependencyInjection
{
    public static IServiceCollection AddCQRS(this IServiceCollection services, Action<CQRSConfiguration> configure)
    {
        var config = new CQRSConfiguration();
        configure(config);

        foreach (var assembly in config.Assemblies)
        {
            var handlerTypes = assembly.GetExportedTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                    )
                );

            foreach (var handlerType in handlerTypes)
            {
                var handlerInterface = handlerType.GetInterfaces()
                    .First(i => i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                services.AddScoped(handlerInterface, handlerType);
            }
        }

        services.AddScoped<IRequestDispatcher, RequestDispatcher>();

        return services;
    }
}