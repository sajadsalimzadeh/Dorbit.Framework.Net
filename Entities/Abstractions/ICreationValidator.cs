using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface ICreationValidator
{
    void ValidateOnCreate(ModelValidationException e);
}