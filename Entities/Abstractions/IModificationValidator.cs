using Dorbit.Exceptions;

namespace Dorbit.Entities.Abstractions;

public interface IModificationValidator
{
    void ValidateOnModify(ModelValidationException e, IServiceProvider sp);
}