using Baitkm.Entities.Base;
using Baitkm.Enums;

namespace Baitkm.Entities
{
    public class Fact : EntityBase
    {
        public int UserId { get; set; }
        public int AnnouncementId { get; set; }
        public ActivityType ActivityType { get; set; }
        public bool IsGuest { get; set; }
        public string AnnouncementPhoto { get; set; }

        public virtual Announcement Announcement { get; set; }
    }
}