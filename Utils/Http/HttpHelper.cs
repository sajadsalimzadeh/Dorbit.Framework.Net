using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Dorbit.Framework.Contracts.Results;
using Dorbit.Framework.Utils.Json;
using Microsoft.Extensions.Caching.Memory;

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
    public HttpClient HttpClient { get; private set; }
    public CookieContainer CookieContainer { get; } = new();
    public CancellationToken CancellationToken { get; set; }

    public event Action OnUnAuthorizedHandler;
    public event Action OnForbiddenHandler;
    public event Action<Exception, HttpModel> OnException;

    public HttpHelper(string baseUrl, HttpMessageHandler handler = null)
    {
        handler ??= new HttpClientHandler()
        {
            Proxy = null,
            UseProxy = false,
            CookieContainer = CookieContainer,
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
        var properties = obj.GetType()
            .GetProperties()
            .Where(p => p.GetValue(obj, null) != null)
            .Select(p =>
            {
                var jsonPropertyAttribute = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                var name = (jsonPropertyAttribute != null ? jsonPropertyAttribute.Name : p.Name);
                return name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null)?.ToString());
            });

        return string.Join("&", properties.ToArray());
    }

    public HttpHelper AddHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    private HttpRequestMessage CreateRequest(HttpHelperRequest httpRequest)
    {
        var request = new HttpRequestMessage(httpRequest.Method, httpRequest.Url);

        if (Username is not null && Password is not null)
        {
            var authenticationString = $"{Username}:{Password}";
            var base64EncodedAuthenticationString =
                Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        if (httpRequest.Parameter is HttpContent content)
        {
            request.Content = content;
        }
        else
        {
            switch (httpRequest.Method.Method.ToLower())
            {
                case "get":
                case "delete":
                case "options":
                    if (httpRequest.Parameter is not null)
                    {
                        var url = httpRequest.Url + (httpRequest.Url.Contains("?") ? "&" : "?") + GetQueryString(httpRequest.Parameter);
                        request.RequestUri = new Uri(url, UriKind.Relative);
                    }

                    break;
                case "post":
                case "put":
                case "patch":
                    if (httpRequest.Parameter is byte[] bytes)
                    {
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        request.Content = fileContent;
                    }
                    else if (httpRequest.Parameter is IEnumerable<KeyValuePair<string, string>> keyValuePairs)
                    {
                        request.Content = new FormUrlEncodedContent(keyValuePairs);
                    }
                    else
                    {
                        if (RequestContentType == ContentType.Json)
                        {
                            var json = JsonSerializer.Serialize(httpRequest.Parameter, JsonSerializerOptions.Web);
                            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        }
                        else if (RequestContentType == ContentType.Xml)
                        {
                            using var writer = new StringWriter();
                            if (httpRequest.Parameter != null)
                            {
                                var serializer = new XmlSerializer(httpRequest.Parameter.GetType());
                                serializer.Serialize(writer, httpRequest.Parameter);
                                request.Content =
                                    new StringContent(writer.ToString(), Encoding.UTF8, "application/xml");
                            }
                        }
                    }

                    break;
            }
        }

        return request;
    }

    public async Task<HttpModel> SendAsync(HttpRequestMessage request)
    {
        if (AuthorizationToken is not null)
        {
            request.Headers.Add("Authorization", AuthorizationToken);
        }

        foreach (var item in _headers)
        {
            request.Headers.Add(item.Key, item.Value);
        }

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
        }

        var httpModelType = new HttpModel<T>()
        {
            Request = httpModel.Request,
            Response = httpModel.Response,
        };

        httpModelType.Content = await httpModel.Response.Content.ReadAsStringAsync(CancellationToken);
        if (httpModel.Response.IsSuccessStatusCode)
        {
            try
            {
                if (ResponseContentType == ContentType.Json)
                {
                    httpModelType.Result = JsonSerializer.Deserialize<T>(httpModelType.Content, JsonSerializerOptions.Web);
                }
                else if (ResponseContentType == ContentType.Xml)
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using var stringReader = new StringReader(httpModelType.Content);
                    httpModelType.Result = (T)serializer.Deserialize(stringReader);
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(
                    new Exception($"{httpModel.Request.Method} {httpModel.Request.RequestUri} -> {httpModel.Response.StatusCode}", ex), httpModel);
            }
        }
        else
        {
            OnException?.Invoke(new Exception($"[{httpModel.Response.StatusCode}]"), httpModel);
        }

        return httpModelType;
    }

    public Task<HttpModel> SendAsync(HttpHelperRequest helperRequest)
    {
        var request = CreateRequest(helperRequest);
        return SendAsync(request);
    }

    public Task<HttpModel<T>> SendAsync<T>(HttpHelperRequest helperRequest)
    {
        var request = CreateRequest(helperRequest);
        return SendAsync<T>(request);
    }

    public async Task<HttpModel<T>> SendAsyncWithCache<T>(HttpHelperRequest helperRequest, string key, TimeSpan timeout)
    {
        if (!App.MemoryCache.TryGetValue(key, out HttpModel<T> value))
        {
            var request = CreateRequest(helperRequest);
            value = await SendAsync<T>(request);
            if (value.Response.IsSuccessStatusCode)
            {
                App.MemoryCache.Set(key, value, timeout);
            }
        }

        return value;
    }

    public Task<HttpModel<T>> GetAsync<T>(string url, object parameter = null) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Get, url) { Parameter = parameter });

    public Task<HttpModel<T>> PostAsync<T>(string url, object parameter) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Post, url) { Parameter = parameter });

    public Task<HttpModel<T>> PutAsync<T>(string url, object parameter) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Put, url) { Parameter = parameter });

    public Task<HttpModel<T>> PatchAsync<T>(string url, object parameter) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Patch, url) { Parameter = parameter });

    public Task<HttpModel<T>> DeleteAsync<T>(string url, object parameter) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Delete, url) { Parameter = parameter });

    public Task<HttpModel<T>> OptionsAsync<T>(string url) =>
        SendAsync<T>(new HttpHelperRequest(HttpMethod.Options, url));

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    protected virtual void RaiseException(Exception arg1, HttpModel arg2)
    {
        OnException?.Invoke(arg1, arg2);
    }
}

public static class HttpHelperExtensions
{
    public static async Task<QueryResult<T>> ToResultAsync<T>(this Task<HttpModel<QueryResult<T>>> httpModel)
    {
        return (await httpModel).Result ?? new QueryResult<T> { Success = false };
    }

    public static async Task<T> ToResultAsync<T>(this Task<HttpModel<T>> httpModel)
    {
        return (await httpModel).Result;
    }

    public static async Task<TR> ToResultAsync<T, TR>(this Task<HttpModel<T>> httpModel, Func<T, TR> select)
    {
        return select(await httpModel.ToResultAsync());
    }
}