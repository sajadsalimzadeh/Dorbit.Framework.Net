using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IValidate
{
    void Validate(ModelValidationException e);
}