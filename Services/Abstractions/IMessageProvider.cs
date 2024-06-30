using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;

namespace Dorbit.Framework.Services.Abstractions;

public interface IMessageProvider<T, TC> where T : MessageRequest
{
    public string Name { get; }
    void Configure(TC configuration);
    Task<QueryResult<string>> SendAsync(T request);
}