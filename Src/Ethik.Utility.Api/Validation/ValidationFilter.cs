using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Ethik.Utility.Api.Validation.Interfaces;

namespace Ethik.Utility.Api.Validation;

public class ValidationFilterConfiguration
{
    public bool HandleValidationResult { get; set; } = true;
}
public class ValidationFilter : IAsyncActionFilter
{
    private readonly ValidationFilterConfiguration _config;

    public ValidationFilter(ValidationFilterConfiguration config) => _config = config;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is IBaseValidatable)
            {
                var modelType = argument.GetType();

                // Construct ValidationRuleBuilder<TModel> dynamically
                var builderType = typeof(ValidationRuleBuilder<>).MakeGenericType(modelType);
                var builder = Activator.CreateInstance(builderType, argument);

                var interfaceType = typeof(IValidatable<>).MakeGenericType(modelType);
                var configureMethod = interfaceType.GetMethod("ConfigureValidation");

                configureMethod?.Invoke(argument, new[] { builder });

                var validateMethod = builderType.GetMethod("Validate");
                try
                {
                    validateMethod?.Invoke(builder, null);
                }
                catch (TargetInvocationException ex) when (ex.InnerException is ValidationException validationEx)
                {
                    if (!_config.HandleValidationResult)
                        throw validationEx;

                    context.Result = new BadRequestObjectResult(new
                    {
                        Error = "Validation Failed",
                        Errors = validationEx.Errors.Select(e => new
                        {
                            e.PropertyName,
                            e.ErrorMessage,
                            e.AttemptedValue
                        })
                    });
                    return;
                }
            }
        }

        await next();
    }

}
