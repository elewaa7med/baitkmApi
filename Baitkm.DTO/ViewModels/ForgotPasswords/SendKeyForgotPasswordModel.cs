using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.ForgotPasswords
{
    public class SendKeyForgotPasswordModel : IViewModel
    {
        [Required]
        public string VerificationTerm { get; set; }
        public string PhoneCode { get; set; }
    }
}