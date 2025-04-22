namespace Ethik.Utility.Api.Validation.Interfaces;

public interface IValidatable<TModel> : IBaseValidatable
{
    void ConfigureValidation(ValidationRuleBuilder<TModel> validator);
}
