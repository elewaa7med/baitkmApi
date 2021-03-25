using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.UserRelated;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Token
{
    public class TokenViewModel : IViewModel
    {
        [Required]
        public string VerificationTerm { get; set; }
        [Required]
        public string Password { get; set; }
        //public VerifiedBy VerifiedBy { get; set; }
    }
}