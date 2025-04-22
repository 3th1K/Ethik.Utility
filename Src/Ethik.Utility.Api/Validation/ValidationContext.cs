namespace Ethik.Utility.Api.Validation;

public class ValidationContext<TRequest>
{
    public TRequest InstanceToValidate { get; }

    public ValidationContext(TRequest instance)
    {
        InstanceToValidate = instance;
    }
}
