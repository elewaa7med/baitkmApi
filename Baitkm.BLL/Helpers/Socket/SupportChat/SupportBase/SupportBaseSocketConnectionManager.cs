using Baitkm.DTO.ViewModels.Helpers.Socket;
using Baitkm.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.SupportChat.SupportBase
{
    public class SupportBaseSocketConnectionManager
    {
        public static readonly ConcurrentDictionary<string, SupportChatConnectionModel> Sockets = new ConcurrentDictionary<string, SupportChatConnectionModel>();

        public static List<SupportChatConnectionModel> GetSocketById(int userId, string deviceId)
        {
            var key = KeyGenerator(userId, false, deviceId);
            return Sockets.Where(x => x.Key.Contains(key)).Select(x => x.Value).ToList();
        }

        public static List<SupportChatConnectionModel> GetAll()
        {
            return Sockets.Values.ToList();
        }

        public static async Task AddSocket(int userId, WebSocket socket, bool isGuest, string deviceId)
        {
            var key = KeyGenerator(userId, isGuest, deviceId);
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                Sockets.TryAdd(key, new SupportChatConnectionModel
                {
                    UserId = userId,
                    WebSocket = socket,
                    UserType = isGuest ? UserType.Guest : UserType.User,
                    DeviceId = deviceId
                });
            else
            {
                if (storedSocket.WebSocket.State == WebSocketState.Open ||
                    storedSocket.WebSocket.State == WebSocketState.None ||
                    storedSocket.WebSocket.State == WebSocketState.Connecting)
                    await storedSocket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "Closed by the WebSocketManager",
                    cancellationToken: CancellationToken.None);
                Sockets.TryRemove(key, out _);
                Sockets.TryAdd(key, new SupportChatConnectionModel
                {
                    UserId = userId,
                    WebSocket = socket,
                    UserType = isGuest ? UserType.Guest : UserType.User,
                    DeviceId = deviceId
                });
            }
        }

        public static async Task RemoveSocket(int userId, bool isGuest, string deviceId)
        {
            var key = KeyGenerator(userId, isGuest, deviceId);
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                return;
            if (storedSocket.WebSocket.State == WebSocketState.Open ||
                storedSocket.WebSocket.State == WebSocketState.None ||
                storedSocket.WebSocket.State == WebSocketState.Connecting)
                await storedSocket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None);
            Sockets.TryRemove(key, out _);
        }

        private static string KeyGenerator(int userId, bool isGuest, string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
                return $"{userId}{isGuest}{deviceId}";
            return $"{userId}{isGuest}";
        }
    }
}
