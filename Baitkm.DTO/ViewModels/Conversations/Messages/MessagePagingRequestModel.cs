using Baitkm.DTO.ViewModels.Helpers.Paging;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class MessagePagingRequestModel : PagingRequestModel
    {
        public int ConversationId { get; set; }
    }
}
