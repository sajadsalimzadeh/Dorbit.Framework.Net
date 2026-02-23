using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class TwilioEmailProvider : IMessageProvider<MessageEmailRequest, ConfigMessageEmailProvider>
{
    private string _apiKey;
    private string _sender;
    private string _senderName;
    
    public string Name => "TwilioEmail";
    
    public void Configure(ConfigMessageEmailProvider configuration)
    {
        _sender = configuration.Sender;
        _senderName = configuration.SenderName;
        _apiKey = configuration.ApiKey.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> SendAsync(MessageEmailRequest request)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(_sender, _senderName);
        var subject = request.Subject;
        var to = new EmailAddress(request.Receiver, "User");
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", request.Body);
        var response = await client.SendEmailAsync(msg);
        var content = await response.Body.ReadAsStringAsync();
        return new QueryResult<string>() { Success = response.IsSuccessStatusCode, Message = content };
    }
}