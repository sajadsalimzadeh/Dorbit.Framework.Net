using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Devor.Framework.Utils.Http
{
    public class HttpClient
    {
        public string Url { get; set; }
        public HttpResponseMessage Response { get; private set; }
        public Action<Exception, HttpRequestMessage, HttpResponseMessage> OnException;

        private CookieContainer cookies;
        private System.Net.Http.HttpClient client;
        private Dictionary<string, string> headers;

        public Action UnAuthorized { get; set; }
        public Action<HttpRequestMessage> OnBeforeSend { get; set; }

        public Func<HttpResponseMessage, bool> IsUnAuthorizedResponse { get; set; } = (res) =>
        {
            return res.StatusCode == HttpStatusCode.Unauthorized;
        };

        public HttpClient(string url)
        {
            Url = url;
            cookies = new CookieContainer();

            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            client = new System.Net.Http.HttpClient(handler);
            headers = new Dictionary<string, string>();
        }

        public IEnumerable<Cookie> GetCookies()
        {
            Uri uri = new Uri(Url);
            return cookies.GetCookies(uri).Cast<Cookie>();
        }

        public HttpClient SetCookie(Cookie cookie)
        {
            cookies.Add(cookie);
            return this;
        }

        public HttpClient AddHeader(string name, string value)
        {
            headers.Add(name, value);
            return this;
        }

        public HttpClient AddHeaderAuthorization(string schema, string parameter)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, parameter);
            return this;
        }

        public HttpClient Clear()
        {
            headers.Clear();
            return this;
        }

        public string GetQueryString(object obj)
        {
            if (obj == null) return null;
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }

        public async Task<T> SendAsync<T>(HttpMethod method, object parameter = null, CancellationToken cancellationToken = default)
        {
            var url = Url;
            StringContent content = null;
            switch (method.Method.ToLower())
            {
                case "get":
                case "delete":
                case "options":
                    url += (Url.Contains("?") ? "&" : "?") + GetQueryString(parameter);
                    break;
                case "post":
                case "put":
                case "patch":
                    content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(parameter), Encoding.UTF8, "application/json");
                    break;
            }

            var request = new HttpRequestMessage(method, url) { Content = content };
            foreach (var item in headers) request.Headers.Add(item.Key, item.Value);
            OnBeforeSend?.Invoke(request);
            Response = await client.SendAsync(request, cancellationToken);
            if (IsUnAuthorizedResponse(Response))
            {
                if (UnAuthorized != null)
                {
                    UnAuthorized();
                    UnAuthorized = null;
                    return await SendAsync<T>(method, parameter, cancellationToken);
                }
            }

            try
            {
                var responseString = Response.Content.ReadAsStringAsync().Result;
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseString);
                return result;
            }
            catch (Exception ex)
            {
                if (OnException is not null)
                    OnException(ex, request, Response);
                return default;
            }
        }
        public Task<T> GetAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Get, parameter);
        public Task<T> PostAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Post, parameter);
        public Task<T> PutAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Put, parameter);
        public Task<T> PatchAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Patch, parameter);
        public Task<T> DeleteAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Delete, parameter);
        public Task<T> OptionsAsync<T>(object parameter = null) => SendAsync<T>(HttpMethod.Options, parameter);

        public T Send<T>(HttpMethod method, object parameter = null) => SendAsync<T>(method, parameter).Result;
        public T Get<T>(object parameter = null) => Send<T>(HttpMethod.Get, parameter);
        public T Post<T>(object parameter = null) => Send<T>(HttpMethod.Post, parameter);
        public T Put<T>(object parameter = null) => Send<T>(HttpMethod.Put, parameter);
        public T Patch<T>(object parameter = null) => Send<T>(HttpMethod.Patch, parameter);
        public T Delete<T>(object parameter = null) => Send<T>(HttpMethod.Delete, parameter);
        public T Options<T>(object parameter = null) => Send<T>(HttpMethod.Options, parameter);
    }
}