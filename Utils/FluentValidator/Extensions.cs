using FluentValidation.Results;

namespace Dorbit.Framework.Utils.FluentValidator;

public static class Extensions
{
    public static ValidationResult Merge(this ValidationResult result, ValidationResult target)
    {
        return new ValidationResult(result.Errors.Concat(target.Errors));
    }
}