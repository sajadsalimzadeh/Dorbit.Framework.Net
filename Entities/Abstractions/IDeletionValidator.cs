using System;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IDeletionValidator
{
    void ValidateOnDelete(ModelValidationException e);
}