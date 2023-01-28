using Devor.Framework.Exceptions;
using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface ICreationValidator
    {
        void ValidateOnCreate(ModelValidationException e, IServiceProvider sp);
    }
}
