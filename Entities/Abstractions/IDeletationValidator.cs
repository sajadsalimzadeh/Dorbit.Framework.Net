using System;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletationValidator
{
    void ValidateOnDelete(ModelValidationException e, IServiceProvider sp);
}