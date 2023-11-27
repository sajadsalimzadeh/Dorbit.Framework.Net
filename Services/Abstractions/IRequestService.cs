namespace Dorbit.Framework.Services.Abstractions;

public interface IRequestService
{
    Guid CorrelationId { get; }
}