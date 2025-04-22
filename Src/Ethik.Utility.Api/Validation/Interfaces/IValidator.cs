using Ethik.Utility.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Ethik.Utility.Api.Validation.Interfaces;

public interface IValidator<TRequest>
{
    ValidationResult Validate(ValidationContext<TRequest> context);
}
