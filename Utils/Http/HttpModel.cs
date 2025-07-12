using System.Net.Http;

namespace Dorbit.Framework.Utils.Http;

public class HttpModel
{
    public HttpRequestMessage Request { get; set; }
    public HttpResponseMessage Response { get; set; }
    public string Content { get; set; }

    public virtual bool IsSuccess => Response?.IsSuccessStatusCode ?? false;
}

public class HttpModel<T> : HttpModel
{
    public T Result { get; set; }

    public override bool IsSuccess => base.IsSuccess && Result != null;
}