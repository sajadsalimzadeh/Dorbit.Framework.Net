using System.Collections.Generic;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using FirebaseAdmin.Messaging;

namespace Dorbit.Framework.Services;

[ServiceRegister]
public class FirebaseService
{
    public Task<string> SendAsync(Message message)
    {
        return FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public Task<BatchResponse> SendAsync(List<Message> messages)
    {
        return FirebaseMessaging.DefaultInstance.SendEachAsync(messages);
    }
}