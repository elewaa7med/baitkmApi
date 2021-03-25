using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingResponseReportFilter : PagingResponseModel<AnnouncementReportModel>, IViewModel
    {
        public IEnumerable<AnnouncementReportModel> AnnouncementResponseFilter { get; set; }
    }
}
