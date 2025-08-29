using System;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Services.Abstractions;
using MailKit;
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
    private string _senderName;
    private string _senderEmail;
    private string _username;
    private string _password;
    private string _apiKey;

    public void Configure(ConfigMessageEmailProvider configuration)
    {
        _server = configuration.Server;
        _port = configuration.Port;
        _senderName = configuration.SenderName;
        _senderEmail = configuration.Sender;
        _username = configuration.Username;
        _password = configuration.Password?.GetDecryptedValue();
        _apiKey = configuration.ApiKey?.GetDecryptedValue();
    }

    public async Task<QueryResult<string>> SendAsync(MessageEmailRequest request)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_senderName, _senderEmail));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
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

        message.Body = new TextPart("plain") { Text = string.Format(request.Body, request.Args ?? []) };
        using var logger = new ProtocolLogger(Console.OpenStandardError());
        using var smtp = new SmtpClient(logger);
        await smtp.ConnectAsync(_server, _port);
        if (_apiKey.IsNotNullOrEmpty()) await smtp.AuthenticateAsync("apikey", _apiKey);
        else if(_password.IsNotNullOrEmpty()) await smtp.AuthenticateAsync(_username, _password);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        return new QueryResult<string>() { Success = true, Data = "" };
    }
}