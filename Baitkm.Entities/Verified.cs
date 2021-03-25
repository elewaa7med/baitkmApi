using Baitkm.Entities.Base;
using Baitkm.Enums.UserRelated;

namespace Baitkm.Entities
{
    public class Verified : EntityBase
    {
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRegistered { get; set; }
        public VerifiedType VerifiedType { get; set; }
        public VerifiedBy VerifiedBy { get; set; }
    }
}