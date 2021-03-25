using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class MessageDeleteModel : IViewModel
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
    }
}
