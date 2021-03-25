using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.SupportChat.SupportBase
{
    public static class SupportBaseMessageHandler
    {
        public static async Task OnDisconnected(int userId, bool isGuest, string deviceId)
        {
            await SupportBaseSocketConnectionManager.RemoveSocket(userId, isGuest, deviceId);
        }

        public static async Task<bool> SendMessageAsync(int conversationId, int userId)
        {
            var convString = conversationId.ToString();
            var success = false;
            var socketObject = SupportBaseSocketConnectionManager.GetSocketById(userId, null);
            if (!socketObject.Any())
                return false;
            foreach (var socket in socketObject)
            {
                if (socket.WebSocket.State != WebSocketState.Open)
                    continue;
                await socket.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(convString),
                        offset: 0,
                        count: convString.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
                success = true;
            }
            return success;
        }

        public static async Task OnConnected(int userId, WebSocket socket, bool isGuest, string deviceId)
        {
            await SupportBaseSocketConnectionManager.AddSocket(userId, socket, isGuest, deviceId);
        }
    }
}
