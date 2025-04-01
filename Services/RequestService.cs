using System;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Services.Abstractions;

namespace Dorbit.Framework.Services;

[ServiceRegister]
internal class RequestService : IRequestService
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}