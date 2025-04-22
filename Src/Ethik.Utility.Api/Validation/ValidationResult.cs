namespace Ethik.Utility.Api.Validation;

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<ValidationFailure> Errors { get; }

    public ValidationResult()
    {
        Errors = new List<ValidationFailure>();
    }

    public ValidationResult(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures.ToList();
    }
}
