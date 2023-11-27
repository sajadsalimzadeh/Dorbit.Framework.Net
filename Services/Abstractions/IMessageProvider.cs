using Dorbit.Framework.Models;
using Dorbit.Framework.Models.Messages;
using Microsoft.Extensions.Configuration;

namespace Dorbit.Framework.Services.Abstractions;

public interface IMessageProvider<T> where T : MessageRequest
{
    public string Name { get; }
    void Configure(IConfiguration configuration);
    Task<OperationResult> Send(T request);
}