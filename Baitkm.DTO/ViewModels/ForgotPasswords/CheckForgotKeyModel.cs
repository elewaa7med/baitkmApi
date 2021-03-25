using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.ForgotPasswords
{
    public class CheckForgotKeyModel : IViewModel
    {
        [Required]
        public string VerificationTerm { get; set; }
        [Required]
        public string Code { get; set; }
        public string PhoneCode { get; set; }
    }
}
