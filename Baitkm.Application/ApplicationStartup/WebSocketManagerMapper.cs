using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.Application.ApplicationStartup
{
    public static class WebSocketManagerMapper
    {
        public static async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
