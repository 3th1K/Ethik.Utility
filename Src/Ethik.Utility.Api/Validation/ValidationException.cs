namespace Ethik.Utility.Api.Validation;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(string message)
        : this(message, Enumerable.Empty<ValidationFailure>())
    {
    }

    public ValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string message, IEnumerable<ValidationFailure> errors, bool appendDefaultMessage)
        : base(appendDefaultMessage ? $"{message} {BuildErrorMessage(errors)}" : message)
    {
        Errors = errors;
    }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = Enumerable.Empty<ValidationFailure>();
    }

    public ValidationException() : base()
    {
        Errors = Enumerable.Empty<ValidationFailure>();
    }

    private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
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