using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Attachments;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AnnouncementRejectModel : IViewModel
    {
        public int? Id { get; set; }
        public string DescriptionEnglish { get; set; }
        public string DescriptionArabian { get; set; }
        public AnnouncementNotificationType NotificationType { get; set; }
    }
}