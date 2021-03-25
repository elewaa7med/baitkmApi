using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.SupportChat
{
    public static class SupportChatMessageHandler
    {
        public static async Task OnDisconnected(int conversationId, int userId, bool isGuest, string deviceId)
        {
            await SupportChatSocketConnectionManager.RemoveSocket(conversationId, userId, isGuest, deviceId);
        }

        public static async Task<bool> SendMessageAsync(int conversationId, int userId, string message, bool isGuest)
        {
            var success = false;
            var socketObject = SupportChatSocketConnectionManager.GetSocketById(conversationId, userId, isGuest, null);
            //var sockets = socketObject.Where(x => x.UserId != userId && x.DeviceId != deviceId).ToList();
            if (!socketObject.Any())
                return false;
            foreach (var socket in socketObject)
            {
                if (socket.WebSocket.State != WebSocketState.Open)
                    continue;
                await socket.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
                        offset: 0,
                        count: message.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
                //if (socket.UserId != userId)
                success = true;
            }
            return success;
        }

        public static async Task OnConnected(int conversationId, int userId, WebSocket socket, bool isGuest, string deviceId)
        {
            await SupportChatSocketConnectionManager.AddSocket(conversationId, userId, socket, isGuest, deviceId);
        }
    }
}
