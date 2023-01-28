using Devor.Framework.Exceptions;
using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IValidator
    {
        void Validate(ModelValidationException e, IServiceProvider sp);
    }
}
