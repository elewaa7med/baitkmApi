using Baitkm.Entities.Base;
using Baitkm.Enums;
using Baitkm.Enums.Attachments;

namespace Baitkm.Entities
{
    public class PersonOtherSetting : EntityBase
    {
        public AreaUnit AreaUnit { get; set; }
        public Language Language { get; set; }
        public int? UserId { get; set; }
        public int? GuestId { get; set; }

        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
    }
}