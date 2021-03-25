using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;

namespace Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages
{
    public class PagingResponseSupportMessageModel : PagingResponseModel<SupportMessageListModel>
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int AnnouncementCount { get; set; }
        public ImageOptimizer Photo { get; set; }
    }
}
