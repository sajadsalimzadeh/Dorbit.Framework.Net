using Devor.Framework.Attributes;
using Devor.Framework.Services.Abstractions;
using System;

namespace Devor.Framework.Services
{
    [ServiceRegisterar]
    internal class RequestService : IRequestService
    {
        public Guid CorrelationId { get; }

        public RequestService()
        {
            CorrelationId = Guid.NewGuid();
        }
    }
}
