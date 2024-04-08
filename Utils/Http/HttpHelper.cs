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

    public string BaseUrl { get; set; }
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
        BaseUrl = baseUrl;

        HttpClient = new HttpClient(handler ?? new HttpClientHandler());
        HttpClient.BaseAddress = new Uri(BaseUrl);
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


    public async Task<HttpModel> SendAsync(string url, HttpMethod method, object parameter = null)
    {
        var request = new HttpRequestMessage();
        request.Method = method;

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
                        request.RequestUri = new Uri((BaseUrl.Contains("?") ? "&" : "?") + GetQueryString(parameter));
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
                            request.Content = new StringContent(JsonConvert.SerializeObject(parameter), Encoding.UTF8, "application/json");
                        }
                        else if (RequestContentType == ContentType.Xml)
                        {
                            await using var writer = new StringWriter();
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
        var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken);
        return new HttpModel()
        {
            Request = request,
            Response = response
        };
    }

    public async Task<HttpModel<T>> SendAsync<T>(string url, HttpMethod method, object parameter = null)
    {
        var httpModel = await SendAsync(url, method, parameter);
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
                return await SendAsync<T>(url, method, parameter);
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

        return httpModelType;
    }

    public Task<HttpModel<T>> GetAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Get, parameter);
    public Task<HttpModel<T>> PostAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Post, parameter);
    public Task<HttpModel<T>> PutAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Put, parameter);
    public Task<HttpModel<T>> PatchAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Patch, parameter);
    public Task<HttpModel<T>> DeleteAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Delete, parameter);
    public Task<HttpModel<T>> OptionsAsync<T>(string url = "", object parameter = null) => SendAsync<T>(url, HttpMethod.Options, parameter);

    public Task<HttpModel> GetAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Get, parameter);
    public Task<HttpModel> PostAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Post, parameter);
    public Task<HttpModel> PutAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Put, parameter);
    public Task<HttpModel> PatchAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Patch, parameter);
    public Task<HttpModel> DeleteAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Delete, parameter);
    public Task<HttpModel> OptionsAsync(string url = "", object parameter = null) => SendAsync(url, HttpMethod.Options, parameter);


    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    protected virtual void RaiseException(Exception arg1, HttpRequestMessage arg2, HttpResponseMessage arg3)
    {
        OnException?.Invoke(arg1, arg2, arg3);
    }
}