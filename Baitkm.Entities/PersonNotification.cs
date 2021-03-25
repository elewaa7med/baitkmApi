using Baitkm.Entities.Base;
namespace Baitkm.Entities
{
    public class PersonNotification : EntityBase
    {
        public int? UserId { get; set; }
        public int? GuestId { get; set; }
        public int? AnnouncementId { get; set; }
        public int? NotificationId { get; set; }
        public int? PushNotificationId { get; set; }
        public int? SenderId { get; set; }
        public bool IsSeen { get; set; }

        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual Announcement Announcement { get; set; }
        public virtual Notification Notification { get; set; }
        public virtual PushNotification PushNotification { get; set; }
    }
}