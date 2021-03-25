using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.Enums;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserFilterModel : PagingRequestModel, IViewModel
    {
        public int? AnnouncementCount { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string IpLocation { get; set; }
        public UserStatusType? UserStatusType { get; set; }
    }
}