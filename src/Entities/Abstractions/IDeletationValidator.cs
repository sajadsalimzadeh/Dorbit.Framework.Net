using Devor.Framework.Exceptions;
using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IDeletationValidator
    {
        void ValidateOnDelete(ModelValidationException e, IServiceProvider sp);
    }
}
