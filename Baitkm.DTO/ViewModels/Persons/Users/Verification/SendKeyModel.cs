using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Persons.Users.Verification
{
    public class SendKeyModel : IViewModel
    {
        public string PhoneCode { get; set; }
        [Required]
        public string VerificationTerm { get; set; }
    }
}
