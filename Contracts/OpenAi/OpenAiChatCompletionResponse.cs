using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dorbit.Framework.Contracts.OpenAi;

public class OpenAiChatCompletionResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("object")]
    public string Object { get; set; }

    [JsonProperty("created")]
    public int Created { get; set; }

    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("choices")]
    public List<OpenAiChatCompletionResponseChoices> Choices { get; set; }

    [JsonProperty("usage")]
    public OpenAiChatCompletionResponseUsage Usage { get; set; }

    [JsonProperty("service_tier")]
    public string ServiceTier { get; set; }

    [JsonProperty("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

public class OpenAiChatCompletionResponseChoiceMessage
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("refusal")]
    public string? Refusal { get; set; }

    [JsonProperty("annotations")]
    public List<object> Annotations { get; set; }
}

public class OpenAiChatCompletionResponseChoices
{
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("message")]
    public OpenAiChatCompletionResponseChoiceMessage Message { get; set; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }
}

public class OpenAiChatCompletionPromptTokensDetails
{
    [JsonProperty("cached_tokens")]
    public int CachedTokens { get; set; }

    [JsonProperty("audio_tokens")]
    public int AudioTokens { get; set; }
}

public class OpenAiChatCompletionTokensDetails
{
    [JsonProperty("reasoning_tokens")]
    public int ReasoningTokens { get; set; }

    [JsonProperty("audio_tokens")]
    public int AudioTokens { get; set; }

    [JsonProperty("accepted_prediction_tokens")]
    public int AcceptedPredictionTokens { get; set; }

    [JsonProperty("rejected_prediction_tokens")]
    public int RejectedPredictionTokens { get; set; }
}

public class OpenAiChatCompletionResponseUsage
{
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonProperty("prompt_tokens_details")]
    public OpenAiChatCompletionPromptTokensDetails PromptTokensDetails { get; set; }

    [JsonProperty("completion_tokens_details")]
    public OpenAiChatCompletionTokensDetails CompletionTokensDetails { get; set; }
}