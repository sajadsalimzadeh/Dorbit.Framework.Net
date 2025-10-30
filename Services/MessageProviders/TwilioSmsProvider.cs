using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class TwilioSmsProvider : IMessageProviderSms
{
    public string Name => "TwilioSms";
    private string _sender;

    public void Configure(ConfigMessageSmsProvider configuration)
    {
        _sender = configuration.Sender;
        TwilioClient.Init(configuration.Username, configuration.ApiKey?.GetDecryptedValue());
    }

    public async Task<QueryResult<string>> SendAsync(MessageSmsRequest request)
    {
        var body = request.Body;
        for (var i = 0; i < request.Args.Length; i++)
        {
            body = body.Replace("{" + i + "}", request.Args[i]);
        }

        var message = await MessageResource.CreateAsync(
            body: body,
            from: new Twilio.Types.PhoneNumber(_sender),
            to: new Twilio.Types.PhoneNumber(request.To));

        return new QueryResult<string>() { Success = !message.ErrorCode.HasValue, Data = "" };
    }

    public Task<long> GetCreditMessageCountAsync()
    {
        throw new System.NotImplementedException();
    }
}