using System.Net.Http;

namespace Devor.Framework.Utils.Http
{
    public delegate void HttpClientOnException(HttpRequestMessage request, HttpResponseMessage response);

}