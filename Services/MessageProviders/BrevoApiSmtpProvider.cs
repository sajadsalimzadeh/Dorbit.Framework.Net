using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using Dorbit.Framework.Utils.Http;
using Serilog;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class BrevoApiSmtpProvider(ILogger logger) : IMessageProvider<MessageEmailRequest, ConfigMessageEmailProvider>
{
    public string Name { get; } = "BrevoSmtpApi";

    private string _sender;
    private string _senderName;
    private string _apiKey;

    private class BrevoResponse
    {
        public string MessageId { get; set; }
    }

    public void Configure(ConfigMessageEmailProvider configuration)
    {
        _sender = configuration.Sender;
        _senderName = configuration.SenderName;
        _apiKey = configuration.ApiKey?.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> SendAsync(MessageEmailRequest request)
    {
        var helper = new HttpHelper("https://api.brevo.com/v3");
        helper.AddHeader("api-key", _apiKey);
        var httpModel = await helper.PostAsync<BrevoResponse>("smtp/email", new
        {
            sender = new
            {
                name = _senderName,
                email = _sender,
            },
            to = new object[]
            {
                new
                {
                    email = request.To
                }
            },
            subject = request.Subject,
            htmlContent = string.Format(request.Body, request.Args ?? [])
        });

        if (httpModel.Result is null)
        {
            logger.Error("Brevo result is null content: {@Content}", httpModel.Content);
            return new QueryResult<string>() { Success = false, Message = "Decode failed" };
        }

        return new QueryResult<string>() { Success = true, Data = httpModel.Result.MessageId };
    }
}