using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Persons.Users
{
    public class UserResetPasswordModel : IViewModel
    {
        public string VerificationTerm { get; set; }
        public string PhoneCode { get; set; }
    }
}
