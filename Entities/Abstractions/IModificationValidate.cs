using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IModificationValidate
{
    void ValidateOnModify(ModelValidationException e);
}