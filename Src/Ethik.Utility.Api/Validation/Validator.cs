using Ethik.Utility.Api.Validation.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Ethik.Utility.Api.Validation;

public abstract class Validator<TRequest> : IValidator<TRequest>
{
    private Action<ValidationRuleBuilder<TRequest>>? _rules;

    protected void AddRules(Action<ValidationRuleBuilder<TRequest>> rules)
    {
        _rules = rules;
    }

    public ValidationResult Validate(ValidationContext<TRequest> context)
    {
        if (_rules is null)
            return new ValidationResult();

        var builder = new ValidationRuleBuilder<TRequest>(context.InstanceToValidate);
        _rules(builder);
        builder.Validate();

        return builder.GetResult();
    }
}