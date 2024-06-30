using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Messages;

namespace Dorbit.Framework.Services.Abstractions;

public interface IMessageProviderSms : IMessageProvider<MessageSmsRequest, ConfigMessageSmsProvider>
{
    Task<long> GetCreditMessageCountAsync();
}