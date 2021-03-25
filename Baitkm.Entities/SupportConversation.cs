using Baitkm.Entities.Base;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class SupportConversation : EntityBase
    {
        public int AdminId { get; set; }
        public int? UserId { get; set; }
        public int? GuestId { get; set; }

        public virtual User Admin { get; set; }
        public virtual User User { get; set; }
        public virtual Guest Guest { get; set; }
        public virtual ICollection<SupportMessage> SupportMessages { get; set; }
    }
}