using Baitkm.DTO.ViewModels.Bases;
using Microsoft.AspNetCore.Http;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class SendMessageModel : IViewModel
    {
        public int AnnouncementId { get; set; } 
        public int ConversationId { get; set; }
        public int? ReplayMessageId { get; set; }
        public string MessageText { get; set; }
        public IFormFile File { get; set; }
    }
}
