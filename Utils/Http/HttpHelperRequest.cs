using System.Collections.Generic;
using System.Net.Http;

namespace Dorbit.Framework.Utils.Http;

public class HttpHelperRequest(HttpMethod method, string url)
{
    public HttpMethod Method { get; set; } = method;
    public string Url { get; } = url;
    public object Parameter { get; set; }
    public Dictionary<string, string> Headers { get; set; }

    public HttpHelperRequest(HttpMethod method, string url, object parameter) : this(method, url)
    {
        Parameter = parameter;
    }

    public HttpHelperRequest(HttpMethod method, string url, object parameter, Dictionary<string, string> headers) : this(method, url)
    {
        Parameter = parameter;
        Headers = headers;
    }
}