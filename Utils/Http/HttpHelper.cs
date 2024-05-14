using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Dorbit.Framework.Utils.Http;

public class HttpHelper : IDisposable
{
    public enum ContentType
    {
        Json = 0,
        Xml = 1,
    }

    private readonly Dictionary<string, string> _headers = new();

    public string AuthorizationToken { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IgnoreUnTrustedCertificate { get; set; } = true;
    public int RemainRetryCount { get; set; } = 10;
    public bool IsRetryAfterUnAuthorized { get; set; } = true;
    public ContentType RequestContentType { get; set; } = ContentType.Json;
    public ContentType ResponseContentType { get; set; } = ContentType.Json;
    public HttpClient HttpClient { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public event Action OnUnAuthorizedHandler;
    public event Action OnForbiddenHandler;
    public event Action<Exception, HttpRequestMessage, HttpResponseMessage> OnException;

    public HttpHelper(string baseUrl, HttpMessageHandler handler = null)
    {
        handler ??= new HttpClientHandler()
        {
            Proxy = null,
            ServerCertificateCustomValidationCallback = delegate { return true; },
        };

        HttpClient = new HttpClient(handler);
        HttpClient.BaseAddress = new Uri(baseUrl + (baseUrl.EndsWith("/") ? "" : "/"));
        HttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        HttpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
    }

    private string GetQueryString(object obj)
    {
        if (obj == null) return null;
        var properties = from p in obj.GetType().GetProperties()
            where p.GetValue(obj, null) != null
            select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null)?.ToString());

        return string.Join("&", properties.ToArray());
    }

    public HttpHelper AddHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    private HttpRequestMessage CreateRequest(string url, HttpMethod method, object parameter = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (AuthorizationToken is not null)
        {
            request.Headers.Add("Authorization", AuthorizationToken);
        }

        if (Username is not null && Password is not null)
        {
            var authenticationString = $"{Username}:{Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        if (parameter is HttpContent content)
        {
            request.Content = content;
        }
        else
        {
            switch (method.Method.ToLower())
            {
                case "get":
                case "delete":
                case "options":
                    if (parameter is not null)
                    {
                        request.RequestUri = new Uri(url + (url.Contains("?") ? "&" : "?") + GetQueryString(parameter));
                    }

                    break;
                case "post":
                case "put":
                case "patch":
                    if (parameter is byte[] bytes)
                    {
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        request.Content = fileContent;
                    }
                    else if (parameter is IEnumerable<KeyValuePair<string, string>> keyValuePairs)
                    {
                        request.Content = new FormUrlEncodedContent(keyValuePairs);
                    }
                    else
                    {
                        if (RequestContentType == ContentType.Json)
                        {
                            var json = JsonConvert.SerializeObject(parameter);
                            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        }
                        else if (RequestContentType == ContentType.Xml)
                        {
                            using var writer = new StringWriter();
                            if (parameter != null)
                            {
                                var serializer = new XmlSerializer(parameter.GetType());
                                serializer.Serialize(writer, parameter);
                                request.Content = new StringContent(writer.ToString(), Encoding.UTF8, "application/xml");
                            }
                        }
                    }

                    break;
            }
        }

        foreach (var item in _headers) request.Headers.Add(item.Key, item.Value);
        return request;
    }

    public async Task<HttpModel> SendAsync(HttpRequestMessage request)
    {
        var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken);
        return new HttpModel()
        {
            Request = request,
            Response = response
        };
    }

    public async Task<HttpModel<T>> SendAsync<T>(HttpRequestMessage request)
    {
        var httpModel = await SendAsync(request);
        if (httpModel.Response.StatusCode == HttpStatusCode.Unauthorized)
        {
            OnUnAuthorizedHandler?.Invoke();
        }

        if (httpModel.Response.StatusCode == HttpStatusCode.Forbidden)
        {
            OnForbiddenHandler?.Invoke();
            if (IsRetryAfterUnAuthorized && RemainRetryCount > 0)
            {
                RemainRetryCount--;
                return await SendAsync<T>(request);
            }

            return default;
        }

        await using var stream = await httpModel.Response.Content.ReadAsStreamAsync(CancellationToken);
        using var reader = new StreamReader(stream);

        var httpModelType = new HttpModel<T>()
        {
            Request = httpModel.Request,
            Response = httpModel.Response,
        };

        try
        {
            if (ResponseContentType == ContentType.Json)
            {
                await using var jsonTextReader = new JsonTextReader(reader);
                httpModelType.Result = new JsonSerializer().Deserialize<T>(jsonTextReader);
            }
            else if (ResponseContentType == ContentType.Xml)
            {
                var serializer = new XmlSerializer(typeof(T));
                httpModelType.Result = (T)serializer.Deserialize(reader);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(await reader.ReadToEndAsync(), ex);
        }

        return httpModelType;
    }

    public Task<HttpModel<T>> GetAsync<T>(string url = "", object parameter = null) => SendAsync<T>(CreateRequest(url, HttpMethod.Get, parameter));
    public Task<HttpModel<T>> PostAsync<T>(string url = "", object parameter = null) => SendAsync<T>(CreateRequest(url, HttpMethod.Post, parameter));
    public Task<HttpModel<T>> PutAsync<T>(string url = "", object parameter = null) => SendAsync<T>(CreateRequest(url, HttpMethod.Put, parameter));

    public Task<HttpModel<T>> PatchAsync<T>(string url = "", object parameter = null) =>
        SendAsync<T>(CreateRequest(url, HttpMethod.Patch, parameter));

    public Task<HttpModel<T>> DeleteAsync<T>(string url = "", object parameter = null) =>
        SendAsync<T>(CreateRequest(url, HttpMethod.Delete, parameter));

    public Task<HttpModel<T>> OptionsAsync<T>(string url = "", object parameter = null) =>
        SendAsync<T>(CreateRequest(url, HttpMethod.Options, parameter));

    public Task<HttpModel> GetAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Get, parameter));
    public Task<HttpModel> PostAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Post, parameter));
    public Task<HttpModel> PutAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Put, parameter));
    public Task<HttpModel> PatchAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Patch, parameter));
    public Task<HttpModel> DeleteAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Delete, parameter));
    public Task<HttpModel> OptionsAsync(string url = "", object parameter = null) => SendAsync(CreateRequest(url, HttpMethod.Options, parameter));

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    protected virtual void RaiseException(Exception arg1, HttpRequestMessage arg2, HttpResponseMessage arg3)
    {
        OnException?.Invoke(arg1, arg2, arg3);
    }
}

public static class HttpHelperExtensions
{
    public static async Task<T> ToResultAsync<T>(this Task<HttpModel<T>> httpModel)
    {
        return (await httpModel).Result;
    }

    public static async Task<TR> ToResultAsync<T, TR>(this Task<HttpModel<T>> httpModel, Func<T, TR> select)
    {
        return select(await httpModel.ToResultAsync());
    }
}