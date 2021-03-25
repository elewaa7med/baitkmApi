using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Conversations;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class SendMessageResponseModel : IViewModel
    {
        public int ConversationId { get; set; }
        public int MessageId { get; set; }
        public string MessageText { get; set; }
        public MessageBodyType MessageBodyType { get; set; }
    }
}
