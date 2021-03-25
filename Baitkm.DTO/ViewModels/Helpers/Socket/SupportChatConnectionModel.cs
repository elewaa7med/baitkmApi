using Baitkm.Enums;
using System.Net.WebSockets;

namespace Baitkm.DTO.ViewModels.Helpers.Socket
{
    public class SupportChatConnectionModel
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public WebSocket WebSocket { get; set; }
        public UserType UserType { get; set; }
        public string DeviceId { get; set; }
    }
}
