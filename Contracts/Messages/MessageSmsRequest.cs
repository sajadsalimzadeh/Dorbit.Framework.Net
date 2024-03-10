﻿namespace Dorbit.Framework.Contracts.Messages;

public class MessageSmsRequest : MessageRequest
{
    public string To { get; set; }
    public string Text { get; set; }
    public string[] Args { get; set; }
}