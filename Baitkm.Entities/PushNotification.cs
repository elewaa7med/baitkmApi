using Baitkm.Entities.Base;
using Baitkm.Enums.Notifications;
using System;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class PushNotification : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime SendingDate { get; set; }
        public PushNotificationUserType? PushNotificationUserType { get; set; }
        public PushNotificationStatusType PushNotificationStatusType { get; set; }
        public PushNotificationActionType PushNotificationActionType { get; set; }

        public virtual ICollection<PushNotificationTranslate> PushNotificationTranslates { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
    }
}