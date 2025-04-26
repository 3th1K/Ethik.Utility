using Ethik.Utility.Api.Validation;
using Ethik.Utility.Api.Validation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ethik.Utility.Api.DependencyInjection;

public static class ValidationDependencyInjection
{
    public static void AddAutoValidation(this IServiceCollection services, Action<ValidationFilterConfiguration> configure)
    {
        var config = new ValidationFilterConfiguration();
        configure(config);

        services.AddSingleton(config);

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
    }
    public static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new { ValidatorType = type, InterfaceType = i }))
            .ToList();

        foreach (var validator in validatorTypes)
        {
            services.AddTransient(validator.InterfaceType, validator.ValidatorType);
        }

        return services;
    }
}
