using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class SmtpProvider : IMessageProvider<MessageEmailRequest, ConfigMessageEmailProvider>
{
    public string Name => "Smtp";

    private string _server;
    private short _port;
    private string _senderEmail;
    private string _username;
    private string _password;

    public void Configure(ConfigMessageEmailProvider configuration)
    {
        _server = configuration.Server;
        _port = configuration.Port;
        _senderEmail = configuration.Sender;
        _username = configuration.Username;
        _password = configuration.Password.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> Send(MessageEmailRequest request)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_senderEmail);
        email.To.Add(MailboxAddress.Parse(request.To));
        email.Subject = request.Subject;
        var builder = new BodyBuilder();
        if (request.Attachments != null)
        {
            foreach (var file in request.Attachments)
            {
                if (file.Stream.Length > 0)
                {
                    var bytes = new byte[file.Stream.Length];
                    await file.Stream.WriteAsync(bytes, 0, bytes.Length);
                    builder.Attachments.Add(file.Name, bytes, ContentType.Parse(file.ContentType));
                }
            }
        }

        builder.HtmlBody = string.Format(request.Body, request.Args);
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_server, _port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_username, _password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return new QueryResult<string>() { Success = true, Data = "" };
    }
}