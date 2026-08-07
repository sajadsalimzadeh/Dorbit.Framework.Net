using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dorbit.Framework.Extensions;

public static class WebSocketExtensions
{
    public static Task SendAsync(this ClientWebSocket ws, string message, CancellationToken cancellationToken = default)
    {
        var sendBuffer = Encoding.UTF8.GetBytes(message);
        return ws.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }
    
    public static Task SendAsJsonAsync(this ClientWebSocket ws, object obj, CancellationToken cancellationToken = default)
    {
        return ws.SendAsync(JsonSerializer.Serialize(obj), cancellationToken);
    }
}