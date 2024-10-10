using Ethik.Utility.Api.Models;

namespace Ethik.Utility.Api.Exceptions;

public class ApiValidationException : Exception
{
    public IEnumerable<ApiValidationFailure> Errors { get; }

    public ApiValidationException(string message)
        : this(message, Enumerable.Empty<ApiValidationFailure>())
    {
    }

    public ApiValidationException(string message, IEnumerable<ApiValidationFailure> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ApiValidationException(string message, IEnumerable<ApiValidationFailure> errors, bool appendDefaultMessage)
        : base(appendDefaultMessage ? $"{message} {BuildErrorMessage(errors)}" : message)
    {
        Errors = errors;
    }

    public ApiValidationException(IEnumerable<ApiValidationFailure> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    public ApiValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = Enumerable.Empty<ApiValidationFailure>();
    }

    public ApiValidationException() : base()
    {
        Errors = Enumerable.Empty<ApiValidationFailure>();
    }

    private static string BuildErrorMessage(IEnumerable<ApiValidationFailure> errors)
    {
        IEnumerable<string>? displayedErrors = errors.Take(5).Select(x =>
            $"{Environment.NewLine} -- Property: {x.PropertyName}, Error: {x.ErrorMessage}, Severity: {x.Severity}");

        string message = "Validation failed for the following fields:" + string.Join(string.Empty, displayedErrors);

        if (errors.Count() > 5)
        {
            message += $"{Environment.NewLine}...and {errors.Count() - 5} more.";
        }

        return message;
    }
}