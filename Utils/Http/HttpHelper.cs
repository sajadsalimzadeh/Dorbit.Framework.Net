using System.Net;
using System.Net.Http.Headers;
using System.Text;
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

    private Dictionary<string, string> _headers = new();

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

    public event Action OnUnAuthorizedHandler;
    public event Action OnForbiddenHandler;
    public event Action<Exception, HttpRequestMessage, HttpResponseMessage> OnException;


    public HttpHelper(string baseUrl, HttpMessageHandler handler = null)
    {
        BaseUrl = baseUrl;

        HttpClient = (handler is null ? new HttpClient() : new HttpClient(handler));
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


    public async Task<HttpModel> SendAsync(HttpMethod method, object parameter = null, CancellationToken cancellationToken = default)
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
            var base64EncodedAuthenticationString =
                Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

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
                    var json = JsonConvert.SerializeObject(parameter);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                break;
        }

        foreach (var item in _headers) request.Headers.Add(item.Key, item.Value);
        return new HttpModel()
        {
            Request = request,
            Response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
        };
    }

    public async Task<HttpModel<T>> SendAsync<T>(HttpMethod method, object parameter = null, CancellationToken cancellationToken = default)
    {
        var httpModel = await SendAsync(method, parameter, cancellationToken);
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
                return await SendAsync<T>(method, parameter);
            }

            return default;
        }

        using var stream = await httpModel.Response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);


        var serializer = new XmlSerializer(typeof(T));
        var httpModelType = new HttpModel<T>()
        {
            Request = httpModel.Request,
            Response = httpModel.Response,
        };

        if (ResponseContentType == ContentType.Json)
        {
            using var jsonTextReader = new JsonTextReader(reader);
            httpModelType.Result = new JsonSerializer().Deserialize<T>(jsonTextReader);
        }
        else if (ResponseContentType == ContentType.Xml)
        {
            httpModelType.Result = (T)serializer.Deserialize(reader);
        }

        return httpModelType;
    }

    public Task<HttpModel<T>> GetAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Get, parameter);
    public Task<HttpModel<T>> PostAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Post, parameter);
    public Task<HttpModel<T>> PutAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Put, parameter);
    public Task<HttpModel<T>> PatchAsync<T>(object parameter = null) => SendAsync<T>(new HttpMethod("Patch"), parameter);
    public Task<HttpModel<T>> DeleteAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Delete, parameter);
    public Task<HttpModel<T>> OptionsAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Options, parameter);

    public HttpModel<T> Send<T>(HttpMethod method, object parameter = null) => SendAsync<T>(method, parameter).Result;
    public HttpModel<T> Get<T>(object parameter = null) => Send<T>(HttpMethod.Get, parameter);
    public HttpModel<T> Post<T>(object parameter = null) => Send<T>(HttpMethod.Post, parameter);
    public HttpModel<T> Put<T>(object parameter = null) => Send<T>(HttpMethod.Put, parameter);
    public HttpModel<T> Patch<T>(object parameter = null) => Send<T>(new HttpMethod("Patch"), parameter);
    public HttpModel<T> Delete<T>(object parameter = null) => Send<T>(HttpMethod.Delete, parameter);
    public HttpModel<T> Options<T>(object parameter = null) => Send<T>(HttpMethod.Options, parameter);

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    protected virtual void RaiseException(Exception arg1, HttpRequestMessage arg2, HttpResponseMessage arg3)
    {
        OnException?.Invoke(arg1, arg2, arg3);
    }
}