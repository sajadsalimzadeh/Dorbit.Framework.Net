using Dorbit.Models;
using Dorbit.Models.Messages;
using Microsoft.Extensions.Configuration;

namespace Dorbit.Services.Abstractions;

public interface IMessageProvider<T> where T : MessageRequest
{
    public string Name { get; }
    void Configure(IConfiguration configuration);
    Task<OperationResult> Send(T request);
}