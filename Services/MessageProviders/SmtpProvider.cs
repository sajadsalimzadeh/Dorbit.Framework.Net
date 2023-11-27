using Dorbit.Framework.Attributes;
using Dorbit.Framework.Models;
using Dorbit.Framework.Models.Messages;
using Dorbit.Framework.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Dorbit.Framework.Services.MessageProviders;

[ServiceRegister]
public class SmtpProvider : IMessageProvider<MessageEmailRequest>
{
    public string Name => "Smtp";

    public string Server { get; set; }
    public short Port { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    public void Configure(IConfiguration configuration)
    {
        Server = configuration.GetValue<string>("Server");
        Port = configuration.GetValue<short>("Port");
        SenderName = configuration.GetValue<string>("SenderName");
        SenderEmail = configuration.GetValue<string>("SenderEmail");
        Username = configuration.GetValue<string>("Username");
        Password = configuration.GetValue<string>("Password");
    }

    public async Task<OperationResult> Send(MessageEmailRequest request)
    {
        
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(SenderEmail);
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
        smtp.Connect(Server, Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(Username, Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);

        return new OperationResult(true);
    }
}