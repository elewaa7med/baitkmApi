using Baitkm.Entities.Base;
using Baitkm.Enums;

namespace Baitkm.Entities
{
    public class PushNotificationTranslate : EntityBase
    {
        public int PushNotificationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Language Language { get; set; }

        public virtual PushNotification PushNotification { get; set; }
    }
}