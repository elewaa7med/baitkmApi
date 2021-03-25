using Baitkm.DTO.ViewModels.Bases;
using System;

namespace Baitkm.DTO.ViewModels.Helpers
{
    public class AnnouncementRejectInfo : IViewModel
    {
        public string Title { get; set; }
        public string AnnouncementRejectReason { get; set; }
        public DateTime AnnouncementRejectDate { get; set; }
        //public AnnouncementRejectStatus AnnouncementRejectReason123 { get; set; }
    }
}