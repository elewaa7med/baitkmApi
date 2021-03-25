using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.PhoneCodes
{
    public class AddPhoneCodeModel : IViewModel
    {
        public string Country { get; set; }
        public string Code { get; set; }
    }
}
