using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AnnouncementReportAddModel : IViewModel
    {
        public int AnnouncementId { get; set; }
        public string Description { get; set; }
    }
}
