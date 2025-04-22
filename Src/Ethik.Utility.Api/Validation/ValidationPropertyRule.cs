using System.Text.RegularExpressions;

namespace Ethik.Utility.Api.Validation;

public class ValidationPropertyRule<TProperty, TModel>
{
    private readonly ValidationRuleBuilder<TModel> _parent;
    private readonly string _propertyName;
    private readonly TProperty _value;
    private Func<TProperty, bool>? _predicate;
    private string? _pendingErrorMessage;
    //private Func<TModel, string>? _pendingMessageFunc;


    public ValidationPropertyRule(ValidationRuleBuilder<TModel> parent, string propertyName, TProperty value)
    {
        _parent = parent;
        _propertyName = propertyName;
        _value = value;
    }

    public ValidationPropertyRule<TProperty, TModel> WithMessage(string errorMessage)
    {
        if (!string.IsNullOrEmpty(_pendingErrorMessage))
        {
            _parent.AddError(new ValidationFailure(_propertyName, errorMessage, _value));
            _pendingErrorMessage = null;
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> WithMessage(Func<TModel, string> messageFunc)
    {
        if (!string.IsNullOrEmpty(_pendingErrorMessage))
        {
            _parent.AddError(new ValidationFailure(_propertyName, messageFunc(_parent.Model), _value));
            _pendingErrorMessage = null;
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> GreaterThan(TProperty minValue)
    {
        if (_value is not null && _value is IComparable comparable)
        {
            if (comparable.CompareTo(minValue) <= 0)
            {
                _parent.AddError(new ValidationFailure(_propertyName,
                    $"Value must be greater than {minValue}", _value));
            }
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> LessThanOrEqual(TProperty maxValue)
    {
        if (_value is not null && _value is IComparable comparable)
        {
            if (comparable.CompareTo(maxValue) > 0)
                _parent.AddError(new ValidationFailure(_propertyName,
                    $"Value must be less than or equal to {maxValue}", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> NotEmpty()
    {
        if (_value == null || _value is string str && string.IsNullOrWhiteSpace(str))
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                "Value cannot be empty", _value));
        }

        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> NotNull()
    {
        if (_value == null)
            _parent.AddError(new ValidationFailure(_propertyName,
                "Value cannot be null", _value));

        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MatchesRegex(string pattern)
    {
        if (_value is string strValue && !Regex.IsMatch(strValue, pattern))
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Value does not match the required pattern: {pattern}", _value));
        }

        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> Must(Func<TProperty, bool> predicate)
    {
        _predicate = predicate;

        // If it's invalid, defer adding error until WithMessage is called
        if (!_predicate(_value))
        {
            _pendingErrorMessage = $"Validation failed for {_propertyName}";
        }

        return this;
    }


    public ValidationPropertyRule<TProperty, TModel> MaxLength(int maxLength)
    {
        if (_value is string str && str.Length > maxLength)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Value exceeds maximum length of {maxLength}", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MinLength(int minLength)
    {
        if (_value is string str && str.Length < minLength)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Value must be at least {minLength} characters long", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> EmailAddress()
    {
        if (_value is string email && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                "Invalid email address format", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> Between(TProperty min, TProperty max)
    {
        if (_value is not null && _value is IComparable comparable)
        {
            if (comparable.CompareTo(min) < 0 || comparable.CompareTo(max) > 0)
            {
                _parent.AddError(new ValidationFailure(_propertyName,
                    $"Value must be between {min} and {max}", _value));
            }
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> NotEmptyCollection()
    {
        if (_value is IEnumerable<object> collection && !collection.Any())
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                "Collection cannot be empty", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MinCount(int min)
    {
        if (_value is IEnumerable<object> collection && collection.Count() < min)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Collection must have at least {min} items", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MaxCount(int max)
    {
        if (_value is IEnumerable<object> collection && collection.Count() > max)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Collection must have no more than {max} items", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> EqualTo(TProperty other)
    {
        if (_value != null && !_value.Equals(other))
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Value must be equal to {other}", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> NotEqualTo(TProperty other)
    {
        if (_value != null && _value.Equals(other))
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                $"Value must not be equal to {other}", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MustBeFutureDate()
    {
        if (_value is DateTime dt && dt <= DateTime.UtcNow)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                "Date must be in the future", _value));
        }
        return this;
    }

    public ValidationPropertyRule<TProperty, TModel> MustBePastDate()
    {
        if (_value is DateTime dt && dt >= DateTime.UtcNow)
        {
            _parent.AddError(new ValidationFailure(_propertyName,
                "Date must be in the past", _value));
        }
        return this;
    }

}
