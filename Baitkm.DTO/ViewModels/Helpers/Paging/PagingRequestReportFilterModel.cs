using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Attachments;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingRequestReportFilterModel : PagingRequestModel, IViewModel
    {
        public FilterAnnouncementModel AnnouncementFilter { get; set; }
        public AnnouncementStatus? ReportAnnouncementStatus { get; set; }
    }
}
