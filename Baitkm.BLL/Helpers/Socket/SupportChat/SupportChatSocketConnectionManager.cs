using Baitkm.DTO.ViewModels.Helpers.Socket;
using Baitkm.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.SupportChat
{
    public class SupportChatSocketConnectionManager
    {
        public static readonly ConcurrentDictionary<string, SupportChatConnectionModel> Sockets = new ConcurrentDictionary<string, SupportChatConnectionModel>();

        public static List<SupportChatConnectionModel> GetSocketById(int conversationId, int userId, bool isGuest, string deviceId)
        {
            var key = KeyGenerator(conversationId, userId, isGuest, deviceId);
            return Sockets.Where(x => x.Key.Contains(key)).Select(x => x.Value).ToList();
        }

        public static List<SupportChatConnectionModel> GetAll()
        {
            return Sockets.Select(x => x.Value).ToList();
        }

        public static async Task AddSocket(int conversationId, int userId, WebSocket socket, bool isGuest, string deviceId)
        {
            var key = KeyGenerator(conversationId, userId, isGuest, deviceId);
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                Sockets.TryAdd(key, new SupportChatConnectionModel
                {
                    UserId = userId,
                    ConversationId = conversationId,
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
                    ConversationId = conversationId,
                    WebSocket = socket,
                    UserType = isGuest ? UserType.Guest : UserType.User,
                    DeviceId = deviceId
                });
            }
        }

        public static async Task RemoveSocket(int conversationId, int userId, bool isGuest, string deviceId)
        {
            var key = KeyGenerator(conversationId, userId, isGuest, deviceId);
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

        private static string KeyGenerator(int conversationId, int userId, bool isGuest, string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
                return $"{userId}{conversationId}{isGuest}{deviceId}";
            return $"{userId}{conversationId}{isGuest}";
        }
    }
}
