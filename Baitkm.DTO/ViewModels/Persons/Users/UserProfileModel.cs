using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.Subscriptions;
using Baitkm.Enums.UserRelated;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserProfileModel : IViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public ImageOptimizer ProfilePhoto { get; set; }
        public string VerificationTerm { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int UnSeenNotificationCount { get; set; }
        public int ActiveAnnouncementCount { get; set; }
        public int HiddenAnnouncementCount { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public bool IsLocal { get; set; }
        public VerifiedBy VerifiedBy { get; set; }
        public OsType OsType { get; set; }
        public int ConversationId { get; set; }
        public int UnreadConversationCount { get; set; }
        public List<SubscriptionsType> Subscriptions { get; set; }
        public int MyAnnouncementCount { get; set; }
        public int SaveFilterCount { get; set; }
        public int FavoriteCount { get; set; }
        public SocialLoginProvider LoginProvider { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
    }
}