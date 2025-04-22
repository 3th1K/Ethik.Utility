using System.Linq.Expressions;

namespace Ethik.Utility.Api.Validation;

public class ValidationRuleBuilder<TModel>
{
    private readonly List<ValidationFailure> _errors = new();
    private readonly TModel _model;
    public TModel Model
    {
        get
        {
            return _model;
        }
    }

    public ValidationRuleBuilder(TModel model) => _model = model;

    public ValidationPropertyRule<TProperty, TModel> RuleFor<TProperty>(Expression<Func<TModel, TProperty>> propertySelector)
    {
        var memberExpression = (MemberExpression)propertySelector.Body;
        var propertyName = memberExpression.Member.Name;
        var value = propertySelector.Compile().Invoke(_model);

        return new ValidationPropertyRule<TProperty, TModel>(this, propertyName, value);
    }

    public void AddError(ValidationFailure error) => _errors.Add(error);

    public void Validate()
    {
        if (_errors.Count > 0)
            throw new ValidationException(_errors);
    }
    public ValidationResult GetResult()
    {
        return new ValidationResult(_errors);
    }
}
