using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IModificationValidator
{
    void ValidateOnModify(ModelValidationException e);
}