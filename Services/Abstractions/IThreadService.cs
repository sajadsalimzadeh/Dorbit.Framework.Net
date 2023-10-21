namespace Dorbit.Services.Abstractions;

public interface IThreadService
{
    Thread MainThread { get; internal set; }
}