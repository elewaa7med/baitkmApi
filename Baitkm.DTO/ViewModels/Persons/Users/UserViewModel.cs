using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums;
using Newtonsoft.Json;
using System;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserViewModel : IViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        public int CityId { get; set; }
        public UserStatusType UserStatusType { get; set; }
        public int AnnouncementCount { get; set; }
        public ImageOptimizer ProfilePhoto { get; set; }
        public string VerificationTerm { get; set; }
        public bool IsBlocked { get; set; }
        public string IpLocation { get; set; }
        public string Sub { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
    }
}
