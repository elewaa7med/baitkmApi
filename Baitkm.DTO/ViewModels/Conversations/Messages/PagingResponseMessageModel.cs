using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Helpers.Paging;

namespace Baitkm.DTO.ViewModels.Conversations.Messages
{
    public class PagingResponseMessageModel : PagingResponseModel<MessageListModel>
    {
        public AnnouncementViewModel Announcement { get; set; }
    }
}
