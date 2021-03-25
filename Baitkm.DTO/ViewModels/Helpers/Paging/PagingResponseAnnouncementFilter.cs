using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingResponseAnnouncementFilter : PagingResponseModel<AnnouncementListViewModel>, IViewModel 
    {
        public List<AnnouncementListViewModel> AnnouncementFilter { get; set; }
        public int ModelFilterCount { get; set; }
    }
}
