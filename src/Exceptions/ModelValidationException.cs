using Devor.Framework.Exceptions;
using System;
using System.Collections.Generic;

namespace Devor.Framework.Exceptions
{
    public class ModelValidationException : Exception
    {
        public List<ModelValidationExceptionItem> Errors { get; } = new List<ModelValidationExceptionItem>();

        public ModelValidationException Add(string field, ValidationMessage message)
        {
            Errors.Add(new ModelValidationExceptionItem(field, message));
            return this;
        }

        public ModelValidationException Add(string field, string message)
        {
            Errors.Add(new ModelValidationExceptionItem(field, message));
            return this;
        }

        public void ThrowIfHasError()
        {
            if (Errors.Count > 0) throw this;
        }
    }
    public class ModelValidationExceptionItem : OperationException
    {
        public string Field { get; set; }
        public ModelValidationExceptionItem(string field, string message) : base(message)
        {
            Field = field;
        }
        public ModelValidationExceptionItem(string field, ValidationMessage e) : base(e)
        {
            Field = field;
        }
    }
}
