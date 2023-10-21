using Dorbit.Attributes;
using Dorbit.Services.Abstractions;

namespace Dorbit.Services
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
