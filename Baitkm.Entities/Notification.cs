using Baitkm.Entities.Base;
using Baitkm.Enums.Attachments;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class Notification : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public AnnouncementNotificationType NotificationType { get; set; }

        public virtual ICollection<NotificationTranslate> NotificationTranslate { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
    }
}