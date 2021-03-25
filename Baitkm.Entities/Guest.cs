using Baitkm.Entities.Base;
using Baitkm.Enums.Notifications;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class Guest : EntityBase
    {
        public string Token { get; set; }
        public string DeviceId { get; set; }
        public OsType OsType { get; set; }
        public int CurrencyId { get; set; }

        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<SaveFilter> SaveFilters { get; set; }
        public virtual ICollection<SupportMessage> SupportMessages { get; set; }
        public virtual ICollection<PersonSetting> PersonSettings { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
        public virtual ICollection<ViewedAnnouncement> ViewedAnnouncements { get; set; }
        public virtual ICollection<SubscribeAnnouncement> SubscribeAnnouncements { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual SupportConversation SupportConversation { get; set; }
        public virtual PersonOtherSetting PersonOtherSetting { get; set; }
    }
}