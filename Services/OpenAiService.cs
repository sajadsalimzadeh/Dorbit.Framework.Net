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

    public void SetToken(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    protected ChatClient GetChatClient()
    {
        var client = new OpenAIClient(_apiKey ?? configOpenAiOptions.Value.ApiKey);
        var chatClient = client.GetChatClient(configOpenAiOptions.Value.Model);
        return chatClient;
    }

    public async Task<string> ChatAsync(string content, string system = null)
    {
        var client = GetChatClient();
        var messages = new List<ChatMessage>();
        messages.Add(ChatMessage.CreateSystemMessage("Don't tell any extra things and just tell the answer shortest possible."));
        if (system.IsNotNullOrEmpty())
        {
            messages.Add(ChatMessage.CreateSystemMessage(system));
        }
        messages.Add(content);
        var chatCompletion = await client.CompleteChatAsync(messages.ToArray());
        var firstContent = chatCompletion.Value.Content.FirstOrDefault();
        if(firstContent is null) return null;
        return firstContent.Text;
    }
}