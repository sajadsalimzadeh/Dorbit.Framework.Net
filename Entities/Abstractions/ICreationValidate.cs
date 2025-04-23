using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ICreationValidate
{
    void ValidateOnCreate(ModelValidationException e);
}