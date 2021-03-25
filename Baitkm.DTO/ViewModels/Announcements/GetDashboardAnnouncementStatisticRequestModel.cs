using Baitkm.DTO.ViewModels.Bases;
using System;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class GetDashboardAnnouncementStatisticRequestModel : IViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}