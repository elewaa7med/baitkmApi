using Baitkm.Entities.Base;
using Baitkm.Enums;

namespace Baitkm.Entities
{
    public class NotificationTranslate : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Language Language { get; set; }
        public int NotificationId { get; set; }

        public virtual Notification Notification { get; set; }
    }
}