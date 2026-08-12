using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Contracts.Messages;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Filters;
using Dorbit.Framework.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dorbit.Framework.Controllers;

[ApiExplorerSettings(GroupName = "framework")]
[Route("Framework/[controller]")]
public class MessagesController(MessageManager messageManager) : BaseController
{
    [HttpPost("Sms"), Auth("Message-Sms")]
    public Task<CommandResult> SendSmsAsync([FromBody] MessageSmsRequest request, CancellationToken cancellationToken = default)
    {
        return messageManager.SendAsync(request, cancellationToken);
    }
    
    [HttpPost("Email"), Auth("Message-Email")]
    public Task<CommandResult> SendEmailAsync([FromBody] MessageEmailRequest request, CancellationToken cancellationToken = default)
    {
        return messageManager.SendAsync(request,  cancellationToken);
    }
    
    [HttpPost("Notification"), Auth("Message-Notification")]
    public Task<CommandResult> SendSmsAsync([FromBody] MessageNotificationRequest request, CancellationToken cancellationToken = default)
    {
        return messageManager.SendAsync(request,  cancellationToken);
    }
}