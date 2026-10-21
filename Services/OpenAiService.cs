using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.OpenAi;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class OpenAiService(IOptions<ConfigOpenAi> configOpenAiOptions)
{
    private string _apiKey = configOpenAiOptions.Value.ApiKey.GetDecryptedValue();

    public void SetApiKey(string value)
    {
        _apiKey = value;
    }

    public async Task<string> ChatAsync(string message, string system = null)
    {
        var messages = new List<OpenAiChatCompletionRequestMessage>();
        messages.Add(new OpenAiChatCompletionRequestMessage("system", "Don't tell any extra things and just tell the answer shortest possible."));
        if (system.IsNotNullOrEmpty())
        {
            messages.Add(new OpenAiChatCompletionRequestMessage("system", system));
        }

        messages.Add(new OpenAiChatCompletionRequestMessage("user", message));

        using var httpHelper = new HttpHelper("https://api.openai.com/v1");
        httpHelper.HttpClientHandler.Proxy = new WebProxy(configOpenAiOptions.Value.Proxy);
        httpHelper.HttpClientHandler.UseProxy = true;
        httpHelper.AuthorizationToken = _apiKey;
        var responseModel = await httpHelper.PostAsync<OpenAiChatCompletionResponse>("chat/completions", new
        {
            Model = configOpenAiOptions.Value.Model,
            Messages = messages
        });
        if (!responseModel.IsSuccess || responseModel.Result == null || responseModel.Result.Choices.IsNullOrEmpty())
            throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);
        return responseModel.Result.Choices.First().Message.Content;
    }

    public async Task<T> ChatAsync<T>(string message, string system = null)
    {
        var result = await ChatAsync(message, system);
        try
        {
            var startIndex = result.IndexOf("{", StringComparison.Ordinal);
            var endIndex = result.LastIndexOf("}", StringComparison.Ordinal) + 1;
            result = result.Substring(startIndex, endIndex - startIndex);
            result = result.Replace("\n", "");
            result = result.Replace("\t", "");
            return JsonConvert.DeserializeObject<T>(result);
        }
        catch (Exception)
        {
            throw new Exception("Deserialize: " + result);
        }
    }
}