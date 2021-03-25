using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingRequestAnnouncementFilterModel : PagingRequestModel, IViewModel
    {
        public FilterAnnouncementModel AnnouncementFilter { get; set; }
    }
}
