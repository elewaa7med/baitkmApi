using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.Chat
{
    public static class ChatMessageHandler
    {
        public static async Task OnDisconnected(int conversationId, int userId, string deviceId)
        {
            await ChatSocketConnectionManager.RemoveSocket(conversationId, userId, deviceId);
        }

        public static async Task<bool> SendMessageAsync(int conversationId, int userId, string message, string deviceId)
        {
            var socketObject = ChatSocketConnectionManager.GetSocketById(conversationId, userId, deviceId);
            var sockets = socketObject.Where(x => x.UserId != userId && x.DeviceId != deviceId).ToList();
            if (!sockets.Any())
                return false;
            var success = false;
            foreach (var socket in sockets)
            {
                if (socket.WebSocket.State != WebSocketState.Open)
                    continue;
                await socket.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
                        offset: 0,
                        count: message.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
                if (socket.UserId != userId)
                    success = true;
            }
            return success;
        }

        public static async Task OnConnected(int conversationId, int userId, WebSocket socket, string deviceId)
        {
            await ChatSocketConnectionManager.AddSocket(conversationId, userId, deviceId, socket);
        }
    }
}
