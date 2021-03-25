using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Conversations;
using Microsoft.AspNetCore.Http;

namespace Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages
{
    public class SendSupportMessageModel : IViewModel
    {
        public int ConversationId { get; set; }
        public string MessageText { get; set; }
        public int? ReplayMessageId { get; set; }
        public SupportMessageBodyType SupportMessageBodyType { get; set; }
        public IFormFile File { get; set; }
    }
}