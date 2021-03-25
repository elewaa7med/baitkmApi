using Baitkm.Entities.Base;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.UserRelated;
using System;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class User : EntityBase
    {
        public string FullName { get; set; }
        public string ProfilePhoto { get; set; }
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsBanned { get; set; }
        public int? CityId { get; set; }
        public VerifiedBy VerifiedBy { get; set; }
        public string ForgotKey { get; set; }
        public VerifiedType ForgotPasswordVerified { get; set; }
        public Role RoleEnum { get; set; }
        public UserStatusType UserStatusType { get; set; }
        public OsType OsType { get; set; }
        public bool IsLocal { get; set; }
        public DateTime UnBlockDate { get; set; }
        public int CurrencyId { get; set; }

        public virtual ICollection<Password> Passwords { get; set; }
        public virtual ICollection<DeviceToken> DeviceTokens { get; set; }
        public virtual ICollection<PersonSetting> PersonSettings { get; set; }
        public virtual ICollection<Favourite> Favourites { get; set; }
        public virtual ICollection<Announcement> Announcements { get; set; }
        public virtual ICollection<SaveFilter> SaveFilters { get; set; }
        public virtual ICollection<Conversation> CreatedConversations { get; set; }
        public virtual ICollection<Conversation> QuestionedConversations { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<SupportMessage> SupportMessages { get; set; }
        public virtual ICollection<SupportConversation> SupportConversations { get; set; }
        public virtual ICollection<Statistic> Statistics { get; set; }
        public virtual ICollection<PersonNotification> PersonNotifications { get; set; }
        public virtual ICollection<ViewedAnnouncement> ViewedAnnouncements { get; set; }
        public virtual ICollection<SubscribeAnnouncement> SubscribeAnnouncements { get; set; }
        public virtual ICollection<AnnouncementReport> AnnouncementReports { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual SupportConversation SupportConversation { get; set; }
        public virtual UserLocation UserLocation { get; set; }
        public virtual City City { get; set; }
        public virtual PersonOtherSetting PersonOtherSetting { get; set; }
    }
}