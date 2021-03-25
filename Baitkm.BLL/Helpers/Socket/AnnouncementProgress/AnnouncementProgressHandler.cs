using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.AnnouncementProgress
{
    public static class AnnouncementProgressHandler
    {
        public static async Task OnDisconnected(int userId, string deviceId)
        {
            await AnnouncementProgressSocketConnectionManager.RemoveSocket(userId, deviceId);
        }

        public static async Task<bool> SendMessageAsync(int userId, string message)
        {
            var success = false;
            var socketObject = AnnouncementProgressSocketConnectionManager.GetSocketById(userId);
            if (!socketObject.Any())
                return false;
            foreach (var socket in socketObject)
            {
                if (socket.Socket.State != WebSocketState.Open)
                    continue;
                await socket.Socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.UTF8.GetBytes(message),
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

        public static async Task OnConnected(int userId, WebSocket socket, string deviceId)
        {
            await AnnouncementProgressSocketConnectionManager.AddSocket(userId, socket, deviceId);
        }
    }
}
