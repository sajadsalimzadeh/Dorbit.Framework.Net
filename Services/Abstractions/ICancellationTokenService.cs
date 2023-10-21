namespace Dorbit.Services.Abstractions
{
    public interface ICancellationTokenService
    {
        CancellationToken CancellationToken { get; set; }
    }
}
