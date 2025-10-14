using System;
using System.Linq;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class OpenAiService(IOptions<ConfigOpenAi> configOpenAiOptions)
{
    protected ChatClient GetChatClient()
    {
        var client = new OpenAIClient(configOpenAiOptions.Value.ApiKey);
        var chatClient = client.GetChatClient(configOpenAiOptions.Value.Model);
        return chatClient;
    }

    public async Task<string> ChatAsync(string content)
    {
        var client = GetChatClient();
        var systemMessage = ChatMessage.CreateSystemMessage("Don't tell any extra things and just tell the answer shortest possible.");
        var chatCompletion = await client.CompleteChatAsync(systemMessage, content);
        var firstContent = chatCompletion.Value.Content.FirstOrDefault();
        if(firstContent is null) return null;
        return firstContent.Text;
    }
}