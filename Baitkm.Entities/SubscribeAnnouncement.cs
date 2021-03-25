using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class SubscribeAnnouncement : EntityBase
    {
        public int AnnouncementId { get; set; }
        public int? UserId { get; set; }
        public int? GuestId { get; set; }
        public string Email { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementEstateType AnnouncementEstateType { get; set; }
        public string Address { get; set; }

        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual Announcement Announcement { get; set; }
    }
}