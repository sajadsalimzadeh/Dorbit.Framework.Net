using System.Collections.Generic;

namespace Dorbit.Framework.Contracts.OpenAi;

public class OpenAiChatCompletionRequest
{
    public string Model { get; set; }
    public List<OpenAiChatCompletionRequestMessage> Messages { get; set; }
}

public class OpenAiChatCompletionRequestMessage(string role, string content)
{
    public string Role { get; set; } = role;
    public string Content { get; set; } = content;
}