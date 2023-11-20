namespace Dorbit.Services.Abstractions;

public interface IRequestService
{
    Guid CorrelationId { get; }
}