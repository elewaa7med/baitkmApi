using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class Favourite : EntityBase
    {
        public int? UserId { get; set; }
        public int? GuestId { get; set; }
        public int AnnouncementId { get; set; }

        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual Announcement Announcement { get; set; }
    }
}