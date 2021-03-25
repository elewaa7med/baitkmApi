using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class Rating : EntityBase
    {
        public int AnnouncementId { get; set; }
        public int? UserId { get; set; }
        public int? GuestId { get; set; }
        public int Rat { get; set; }

        public virtual Announcement Announcement { get; set; }
        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
    }
}