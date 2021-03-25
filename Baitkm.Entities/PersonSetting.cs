using Baitkm.Entities.Base;
using Baitkm.Enums.Subscriptions;

namespace Baitkm.Entities
{
    public class PersonSetting : EntityBase
    {
        public SubscriptionsType SubscriptionsType { get; set; }
        public int? UserId { get; set; }
        public int? GuestId { get; set; }

        public virtual Guest Guest { get; set; }
        public virtual User User { get; set; }
    }
}