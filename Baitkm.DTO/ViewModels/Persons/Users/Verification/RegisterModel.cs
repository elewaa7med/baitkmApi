using Baitkm.DTO.ViewModels.Bases;
using System;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Persons.Users.Verification
{
    public class RegisterModel : IViewModel
    {
        //[Required]
        public string FullName { get; set; }
        //[Required]
        public string PhoneEmail { get; set; }
        public string PhoneCode { get; set; }
        public string VerificationTerm { get; set; }
        //[Required]
        public DateTime DateOfBirth { get; set; }
        public int? CityId { get; set; }
        //[Required]
        public string CityName { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "The Password value cannot be lower than 6 characters.")]
        [Compare("Password", ErrorMessage = "Password and confirm password fields do not match")]
        public string ConfirmPassword { get; set; }
    }
}