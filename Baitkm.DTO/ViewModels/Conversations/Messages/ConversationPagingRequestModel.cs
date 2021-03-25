using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers.Paging;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class ConversationPagingRequestModel : PagingRequestModel , IViewModel
    {
        public string Search { get; set; }
    }
}