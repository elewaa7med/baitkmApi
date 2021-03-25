using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums.Attachments;

namespace Baitkm.DTO.ViewModels.Helpers
{
    public class MyAnnouncementListModel : PagingRequestModel
    {
        public AnnouncementStatus? AnnouncementStatus { get; set; }
    }
}
