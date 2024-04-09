using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;

namespace Dorbit.Framework.Services.Abstractions;

public interface IMessageProvider<T> where T : MessageRequest
{
    public string Name { get; }
    void Configure(AppSettingMessageProvider configuration);
    Task<QueryResult<string>> Send(T request);
}