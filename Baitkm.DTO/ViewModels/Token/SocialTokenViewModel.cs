using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Token
{
    public class SocialTokenViewModel
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public SocialLoginProvider Provider { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceId { get; set; }
        public string SocialId { get; set; }
        public OsType OsType { get; set; }
        public int Id { get; set; }
    }
}
