using System.Net.Http.Json;
using Dorbit.Framework.Models;
using Dorbit.Framework.Models.Messages;
using Dorbit.Framework.Services.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Dorbit.Framework.Services.MessageProviders;

public class MeliPayamakProvider : IMessageProvider<MessageSmsRequest>
{
    public string Name => "MeliPayamak";
    public string ApiKey { get; set; }
    public string From { get; set; }
    public string BodyId { get; set; }

    public void Configure(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("ApiKey");
        From = configuration.GetValue<string>("From");
        BodyId = configuration.GetValue<string>("BodyId");
    }

    public Task<OperationResult> Send(MessageSmsRequest request)
    {
        var apiBaseAddress = new Uri("https://console.melipayamak.com");
        using var client = new HttpClient();
        client.BaseAddress = apiBaseAddress;
        if (!string.IsNullOrEmpty(BodyId) && !string.IsNullOrEmpty(request.TemplateId))
        {
            var result = client.PostAsJsonAsync($"api/send/shared/{ApiKey}", new
            {
                bodyId = request.TemplateId,
                to = request.To,
                args = request.Args
            }).Result;
            _ = result.Content.ReadAsStringAsync().Result;
        }
        else
        {
            var result = client.PostAsJsonAsync($"api/send/simple/{ApiKey}", new
            {
                from = From,
                text = request.Text,
                to = request.To,
                args = request.Args
            }).Result;
            _ = result.Content.ReadAsStringAsync().Result;
        }

        return Task.FromResult(new OperationResult());
    }
}