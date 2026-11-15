using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dorbit.Framework.Attributes;
using Dorbit.Framework.Configs;
using Dorbit.Framework.Contracts.OpenAi;
using Dorbit.Framework.Exceptions;
using Dorbit.Framework.Extensions;
using Dorbit.Framework.Utils.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dorbit.Framework.Services;

[ServiceSingletone]
public class OpenAiService(IOptions<ConfigOpenAi> configOpenAiOptions)
{
    private string _apiKey = configOpenAiOptions.Value?.ApiKey?.GetDecryptedValue();

    public void SetApiKey(string value)
    {
        _apiKey = value;
    }

    public async Task<string> ChatAsync(string message, string system = null)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);

        var messages = new List<OpenAiChatCompletionRequestMessage>();
        messages.Add(new OpenAiChatCompletionRequestMessage("system", "Don't tell any extra things and just tell the answer shortest possible."));
        if (system.IsNotNullOrEmpty())
        {
            messages.Add(new OpenAiChatCompletionRequestMessage("system", system));
        }

        messages.Add(new OpenAiChatCompletionRequestMessage("user", message));

        return await CompleteAsync(new OpenAiChatCompletionRequest
        {
            Model = configOpenAiOptions.Value.Model,
            Messages = messages
        });
    }

    public async Task<string> CompleteAsync(OpenAiChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Model ??= configOpenAiOptions.Value.Model;
        using var httpHelper = CreateHttpHelper();
        var responseModel = await httpHelper.PostAsync<OpenAiChatCompletionResponse>("chat/completions", request, cancellationToken);
        if (!responseModel.IsSuccess || responseModel.Result == null || responseModel.Result.Choices.IsNullOrEmpty())
            throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);
        return responseModel.Result.Choices.First().Message.Content;
    }

    public async Task<float[][]> EmbedAsync(string[] texts, string model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);
        using var httpHelper = CreateHttpHelper();
        var response = await httpHelper.PostAsync<OpenAiEmbeddingResponse>("embeddings", new
        {
            model,
            input = texts,
            encoding_format = "float"
        }, cancellationToken);
        var data = response.Result?.Data;
        if (!response.IsSuccess || data == null || data.Count != texts.Length)
            throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);
        var ordered = data.OrderBy(x => x.Index).ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            if (ordered[index].Index != index || ordered[index].Embedding == null ||
                ordered[index].Embedding.Length == 0 || ordered[index].Embedding.Any(x => !float.IsFinite(x)))
                throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);
        }
        return ordered.Select(x => x.Embedding).ToArray();
    }

    protected virtual HttpHelper CreateHttpHelper()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OperationException(FrameworkErrors.ServiceResultNullOrEmpty);
        var endpoint = configOpenAiOptions.Value.ApiUrl;
        var httpHelper = new HttpHelper(string.IsNullOrWhiteSpace(endpoint) ? "https://api.openai.com/v1" : endpoint);
        if (!string.IsNullOrWhiteSpace(configOpenAiOptions.Value.Proxy))
        {
            httpHelper.HttpClientHandler.Proxy = new WebProxy(configOpenAiOptions.Value.Proxy);
            httpHelper.HttpClientHandler.UseProxy = true;
        }
        httpHelper.AuthorizationToken = _apiKey;
        return httpHelper;
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
