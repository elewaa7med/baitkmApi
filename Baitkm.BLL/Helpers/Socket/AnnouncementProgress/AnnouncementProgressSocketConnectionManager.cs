using Baitkm.Infrastructure.Helpers.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.AnnouncementProgress
{
    public class AnnouncementProgressSocketConnectionManager
    {
        public static readonly ConcurrentDictionary<string, ProgressSocketModel> Sockets = new ConcurrentDictionary<string, ProgressSocketModel>();

        public static List<ProgressSocketModel> GetSocketById(int userId)
        {
            return Sockets.Where(x => x.Key.StartsWith(userId.ToString())).Select(x => x.Value).ToList();
        }

        public static async Task AddSocket(int userId, WebSocket socket, string deviceId)
        {
            var key = $"{userId.ToString()}{deviceId}";
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                Sockets.TryAdd(key, new ProgressSocketModel
                {
                    UserId = userId,
                    Socket = socket,
                    DeviceId = deviceId
                });
            else
            {
                if (storedSocket.Socket.State == WebSocketState.Open ||
                    storedSocket.Socket.State == WebSocketState.None ||
                    storedSocket.Socket.State == WebSocketState.Connecting)
                    await storedSocket.Socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "Closed by the WebSocketManager",
                    cancellationToken: CancellationToken.None);
                Sockets.TryRemove(key, out _);
                Sockets.TryAdd(key, new ProgressSocketModel
                {
                    UserId = userId,
                    Socket = socket,
                    DeviceId = deviceId
                });
            }
        }

        public static async Task RemoveSocket(int userId, string deviceId)
        {
            var key = $"{userId.ToString()}{deviceId}";
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                return;
            if (storedSocket.Socket.State == WebSocketState.Open ||
                storedSocket.Socket.State == WebSocketState.None ||
                storedSocket.Socket.State == WebSocketState.Connecting)
                await storedSocket.Socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None);
            Sockets.TryRemove(key, out _);
        }
    }
}
