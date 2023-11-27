namespace Dorbit.Framework.Services.Abstractions;

public interface ICancellationTokenService
{
    CancellationToken CancellationToken { get; set; }
}