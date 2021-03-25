using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.UserRelated;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserDetailsAdminModel : IViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public ImageOptimizer ProfilePhoto { get; set; }
        public string City { get; set; }
        public int CityId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsLocal { get; set; }
        public VerifiedBy VerifiedBy { get; set; }
        public OsType OsType { get; set; }
        public bool IsBlocked { get; set; }
        public int ActiveAnnouncementCount { get; set; }
        public int HiddenAnnouncementCount { get; set; }
        public int UnreadConversationCount { get; set; }
        public string IpAddress { get; set; }
        public string CityByIpAddress { get; set; }
        public string CountryByIpAddress { get; set; }
        public List<UserActivityModel> Activities { get; set; }
        public double AverageDailyUsage { get; set; }
    }
}