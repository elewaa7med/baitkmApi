using Baitkm.Application.ApplicationStartup;
using Baitkm.BLL.Helpers.Socket.AnnouncementProgress;
using Baitkm.BLL.Helpers.Socket.Chat;
using Baitkm.BLL.Helpers.Socket.Chat.Base;
using Baitkm.BLL.Helpers.Socket.SupportChat;
using Baitkm.BLL.Helpers.Socket.SupportChat.SupportBase;
using Baitkm.DAL.Repository.Entities;
using Baitkm.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Baitkm.Application.Middlewares.Socket
{
    public static class SocketMapperHelper
    {
        public static async Task Map(this HttpContext context, IEntityRepository repository)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            context.Request.Query.TryGetValue("conversationId", out var conversationStringValues);
            if (conversationStringValues.Count == 0)
                context.Request.Headers.TryGetValue("conversationId", out conversationStringValues);
            int.TryParse(conversationStringValues, out var conversationId);
            var path = context.Request.Path.Value;
            if (conversationId == 0 && !path.Contains("supportBaseHub") && !path.Contains("baseHub") && !path.Contains("progressHub"))
                return;
            var userName = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var userId = int.Parse(context.User.Claims.Single(u => u.Type == "userId").Value);//check

            context.Request.Headers.TryGetValue("deviceId", out var deviceIdString);
            if (deviceIdString.Count == 0)
                context.Request.Query.TryGetValue("deviceId", out deviceIdString);
            var deviceId = deviceIdString.FirstOrDefault();
            User user = null;
            if (!string.IsNullOrEmpty(userName))
                user = await repository.Filter<User>(x => x.Email == userName // add verfiedBy
                    || (x.PhoneCode + x.Phone) == userName).FirstOrDefaultAsync();
            if (path.Contains("supportChatHub", StringComparison.InvariantCulture))
            {
                var guest = await repository.Filter<Guest>(x => x.DeviceId == deviceId).FirstOrDefaultAsync();
                if (user == null && guest == null)
                    return;
                await SupportChatMessageHandler.OnConnected(conversationId, user?.Id ?? guest.Id, socket, user == null, deviceId);
                await WebSocketManagerMapper.Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Close)
                        await SupportChatMessageHandler.OnDisconnected(conversationId, user?.Id ?? guest.Id, user == null, deviceId);
                });
            }
            else
            {
                if (user == null)
                    return;
                if (path.Contains("chatHub", StringComparison.InvariantCulture))
                {
                    await ChatMessageHandler.OnConnected(conversationId, user.Id, socket, deviceId);
                    await WebSocketManagerMapper.Receive(socket, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                            await ChatMessageHandler.OnDisconnected(conversationId, user.Id, deviceId);
                    });
                }
                if (path.Contains("supportBaseHub", StringComparison.InvariantCulture))
                {
                    await SupportBaseMessageHandler.OnConnected(user.Id, socket, false, deviceId);
                    await WebSocketManagerMapper.Receive(socket, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                            await SupportBaseMessageHandler.OnDisconnected(user.Id, false, deviceId);
                    });
                }
                if (path.Contains("baseHub", StringComparison.InvariantCulture))
                {
                    await BaseMessageHandler.OnConnected(user.Id, socket, false, deviceId);
                    await WebSocketManagerMapper.Receive(socket, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                            await BaseMessageHandler.OnDisconnected(user.Id, false, deviceId);
                    });
                }
                if (path.Contains("progressHub", StringComparison.InvariantCulture))
                {
                    await AnnouncementProgressHandler.OnConnected(user.Id, socket, deviceId);
                    await WebSocketManagerMapper.Receive(socket, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Close)
                            await AnnouncementProgressHandler.OnDisconnected(user.Id, deviceId);
                    });
                }
            }
        }
    }
}
