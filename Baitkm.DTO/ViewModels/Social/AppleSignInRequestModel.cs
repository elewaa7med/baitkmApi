using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Social
{
    public class AppleSignInRequestModel : IViewModel
    {
        public string Token { get; set; }
        public string FullName {get; set;}
    }
}
