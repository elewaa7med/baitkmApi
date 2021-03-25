using System.Net.WebSockets;

namespace Baitkm.DTO.ViewModels.Helpers.Socket
{
    public class ChatConnectionModel
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public WebSocket WebSocket { get; set; }
        public string DeviceId { get; set; }
    }
}
