using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class AnnouncementReport : EntityBase
    {
        public int AnnouncementId { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public AnnouncementStatus ReportAnnouncementStatus { get; set; }

        public virtual User User { get; set; }
        public virtual Announcement Announcement { get; set; }
    }
}