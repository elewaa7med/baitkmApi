using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums.Attachments;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class ReportedAnnouncementRequestModel : PagingRequestModel
    {
        public AnnouncementStatus? ReportAnnouncementStatus { get; set; }
    }
}
