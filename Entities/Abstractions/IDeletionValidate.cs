using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionValidate
{
    void ValidateOnDelete(ModelValidationException e);
}