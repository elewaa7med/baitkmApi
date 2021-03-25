using Baitkm.DTO.ViewModels.Helpers.Socket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Baitkm.BLL.Helpers.Socket.Chat
{
    public static class ChatSocketConnectionManager
    {
        public static readonly ConcurrentDictionary<string, ChatConnectionModel> Sockets = new ConcurrentDictionary<string, ChatConnectionModel>();

        public static List<ChatConnectionModel> GetSocketById(int conversationId, int userId, string deviceId)
        {
            var key = SocketKeyGenerator(userId, conversationId, deviceId);
            return Sockets.Where(x => x.Key.Contains(conversationId.ToString()) && x.Key != key).Select(x => x.Value).ToList();
        }

        public static List<ChatConnectionModel> GetAll()
        {
            return Sockets.Values.ToList();
        }

        public static async Task AddSocket(int conversationId, int userId, string deviceId, WebSocket socket)
        {
            var key = SocketKeyGenerator(userId, conversationId, deviceId);
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                Sockets.TryAdd(key, new ChatConnectionModel
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    WebSocket = socket,
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
                Sockets.TryAdd(key, new ChatConnectionModel
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    WebSocket = socket,
                    DeviceId = deviceId
                });
            }
        }

        public static async Task RemoveSocket(int conversationId, int userId, string deviceId)
        {
            var key = SocketKeyGenerator(userId, conversationId, deviceId);
            Sockets.TryGetValue(key, out var storedSocket);
            if (storedSocket == null)
                return;
            if (storedSocket.WebSocket.State == WebSocketState.Open ||
                storedSocket.WebSocket.State == WebSocketState.None ||
                storedSocket.WebSocket.State == WebSocketState.Connecting)
                await storedSocket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                statusDescription: "Closed by the WebSocketManager",
                cancellationToken: CancellationToken.None);
            Sockets.TryRemove(key, out storedSocket);
        }

        public static string SocketKeyGenerator(int userId, int conversationId, string deviceId)
        {
            return $"{userId}{conversationId}{deviceId}";
        }
    }
}
