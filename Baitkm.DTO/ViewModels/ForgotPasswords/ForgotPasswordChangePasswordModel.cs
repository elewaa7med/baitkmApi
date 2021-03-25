using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.ForgotPasswords
{
    public class ForgotPasswordChangePasswordModel : IViewModel
    {
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Compare("Password", ErrorMessage = "Password and confirm password fields do not match")]
        public string ConfirmPassword { get; set; }
        public string PhoneCode { get; set; }
        public string VerificationTerm { get; set; }
    }
}