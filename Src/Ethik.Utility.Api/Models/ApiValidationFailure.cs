namespace Ethik.Utility.Api.Models;

[Serializable]
public class ApiValidationFailure
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
    public string Severity { get; set; }
    public object? AttemptedValue { get; set; }
    public string ErrorCode { get; set; }
    public Dictionary<string, object> FormattedMessagePlaceholderValues { get; set; }

    public ApiValidationFailure(string propertyName, string errorMessage)
        : this(propertyName, errorMessage, null, new Dictionary<string, object>())
    {
    }

    public ApiValidationFailure(string propertyName, string errorMessage, object? attemptedValue)
        : this(propertyName, errorMessage, attemptedValue, new Dictionary<string, object>())
    {
    }

    public ApiValidationFailure(string propertyName, string errorMessage, object? attemptedValue, Dictionary<string, object> formattedMessagePlaceholderValues)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
        FormattedMessagePlaceholderValues = formattedMessagePlaceholderValues;
        Severity = "Error"; // Default severity
        ErrorCode = "GENERIC_VALIDATION_ERROR"; // Default error code
    }

    public ApiValidationFailure(string propertyName, string errorMessage, object? attemptedValue, Dictionary<string, object> formattedMessagePlaceholderValues, string severity, string errorCode)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
        FormattedMessagePlaceholderValues = formattedMessagePlaceholderValues;
        Severity = severity;
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        return ErrorMessage ?? string.Empty;
    }
}