using System.Net.WebSockets;

namespace Baitkm.Infrastructure.Helpers.Models
{
    public class ProgressSocketModel
    {
        public int UserId { get; set; }
        public WebSocket Socket { get; set; }
        public string DeviceId { get; set; }
    }
}
