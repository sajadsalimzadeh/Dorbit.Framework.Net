using Dorbit.Framework.Contracts;

namespace Dorbit.Framework.Entities;

public class Message : Entity
{
    public MessageType Type { get; set; }
    public byte TryRemain { get; set; }
    public MessageState State { get; set; }
    public string Body { get; set; }
}