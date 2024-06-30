using System;
using Dorbit.Framework.Exceptions;

namespace Dorbit.Framework.Entities.Abstractions;

public interface IValidator
{
    void Validate(ModelValidationException e);
}