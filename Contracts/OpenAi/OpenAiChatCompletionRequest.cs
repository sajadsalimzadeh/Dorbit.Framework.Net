using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dorbit.Framework.Contracts.OpenAi;

public class OpenAiChatCompletionRequest
{
    public string Model { get; set; }
    public List<OpenAiChatCompletionRequestMessage> Messages { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Store { get; set; }

    [JsonPropertyName("max_completion_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxCompletionTokens { get; set; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiChatCompletionResponseFormat ResponseFormat { get; set; }
}

public class OpenAiChatCompletionResponseFormat
{
    public string Type { get; set; }
}

public class OpenAiChatCompletionRequestMessage(string role, string content)
{
    public string Role { get; set; } = role;
    public string Content { get; set; } = content;
}
