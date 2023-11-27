namespace Dorbit.Framework.Utils.Http;

public class HttpModel
{
    public HttpRequestMessage Request { get; set; }
    public HttpResponseMessage Response { get; set; }
}

public class HttpModel<T> : HttpModel
{
    public T Result { get; set; }
}