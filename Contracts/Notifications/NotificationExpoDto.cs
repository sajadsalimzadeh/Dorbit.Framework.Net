using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.Notifications;

public class NotificationExpoDto
{
    [JsonPropertyName("to")]
    public string Token { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Data { get; set; }
}