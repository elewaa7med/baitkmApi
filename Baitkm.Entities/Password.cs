using Baitkm.Entities.Base;
using Baitkm.Enums;

namespace Baitkm.Entities
{
    public class Password : EntityBase
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public SocialLoginProvider LoginProvider { get; set; }
        public string UniqueIdentifier { get; set; }

        public virtual User User { get; set; }
    }
}