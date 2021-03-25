using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Token
{
    public class TokenVerifyViewModel : IViewModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string VerificationTerm { get; set; }
        public string PhoneCode { get; set; }
    }
}