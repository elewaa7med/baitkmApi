using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums;

namespace Baitkm.DTO.ViewModels.Social
{
    public class SocialLoginModel : IViewModel
    {
        public string Token { get; set; }
        public SocialLoginProvider Provider { get; set; }
    }
}
