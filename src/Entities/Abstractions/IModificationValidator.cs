using Devor.Framework.Exceptions;
using System;

namespace Devor.Framework.Entities.Abstractions
{
    public interface IModificationValidator
    {
        void ValidateOnModify(ModelValidationException e, IServiceProvider sp);
    }
}
