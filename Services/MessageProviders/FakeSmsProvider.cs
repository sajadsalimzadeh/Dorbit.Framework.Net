using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Dorbit.Framework.Services.MessageProviders;

/// <summary>Local SMS stand-in: logs the message (including OTP args) and reports success.</summary>
[ServiceRegister]
public class FakeSmsProvider(ILogger<FakeSmsProvider> logger) : IMessageProviderSms
{
    public string Name => "Fake";

    public void Configure(ConfigMessageSmsProvider configuration)
    {
    }

    public Task<QueryResult<string>> SendAsync(MessageSmsRequest request, CancellationToken cancellationToken = default)
    {
        var body = request.Body ?? "";
        var args = request.Args ?? [];
        for (var i = 0; i < args.Length; i++)
        {
            body = body.Replace("{" + i + "}", args[i] ?? "");
        }

        logger.LogWarning(
            "Fake SMS to {Receiver}: templateId={TemplateId} body={Body} args=[{Args}]",
            request.Receiver,
            request.TemplateId,
            body,
            string.Join(", ", args));

        return Task.FromResult(new QueryResult<string> { Success = true, Data = "fake" });
    }

    public Task<long> GetCreditMessageCountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(9999L);
}
