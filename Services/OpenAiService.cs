using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Extensions;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class OpenAiService(IOptions<ConfigOpenAi> configOpenAiOptions)
{
    private string _apiKey;

    public void SetApiKey(string value)
    {
        _apiKey = value;
    }

    private ChatClient GetChatClient()
    {
        var client = new OpenAIClient(_apiKey ?? configOpenAiOptions.Value.ApiKey.GetDecryptedValue());
        var chatClient = client.GetChatClient(configOpenAiOptions.Value.Model);
        return chatClient;
    }

    public async Task<string> ChatAsync(string content, string system = null, params ChatMessageContentPart[]  extraContentParts)
    {
        var client = GetChatClient();
        var messages = new List<ChatMessage>();
        messages.Add(ChatMessage.CreateSystemMessage("Don't tell any extra things and just tell the answer shortest possible."));
        if (system.IsNotNullOrEmpty())
        {
            messages.Add(ChatMessage.CreateSystemMessage(system));
        }

        var contentParts = new List<ChatMessageContentPart>();
        
        contentParts.Add(ChatMessageContentPart.CreateTextPart(content));
        contentParts.AddRange(extraContentParts);
        
        messages.Add(ChatMessage.CreateUserMessage(contentParts));
        var chatCompletion = await client.CompleteChatAsync(messages.ToArray());
        var firstContent = chatCompletion.Value.Content.FirstOrDefault();
        if(firstContent is null) return null;
        return firstContent.Text;
    }
}