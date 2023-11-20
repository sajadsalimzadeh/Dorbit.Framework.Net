using Dorbit.Exceptions;

namespace Dorbit.Entities.Abstractions;

public interface IValidator
{
    void Validate(ModelValidationException e, IServiceProvider sp);
}