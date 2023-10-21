using Dorbit.Exceptions;

namespace Dorbit.Entities.Abstractions
{
    public interface ICreationValidator
    {
        void ValidateOnCreate(ModelValidationException e, IServiceProvider sp);
    }
}
