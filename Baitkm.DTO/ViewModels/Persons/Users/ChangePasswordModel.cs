using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class ChangePasswordModel : IViewModel
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Compare("NewPassword",ErrorMessage = "Confirm password doesn't match, Type again!")]
        public string ConfirmPassword { get; set; }
    }
}