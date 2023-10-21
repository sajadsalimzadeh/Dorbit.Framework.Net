using Dorbit.Exceptions;

namespace Dorbit.Entities.Abstractions
{
    public interface IDeletationValidator
    {
        void ValidateOnDelete(ModelValidationException e, IServiceProvider sp);
    }
}
