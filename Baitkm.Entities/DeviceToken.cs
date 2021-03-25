using Baitkm.Entities.Base;
using Baitkm.Enums.Notifications;

namespace Baitkm.Entities
{
    public class DeviceToken : EntityBase
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public string DeviceId { get; set; }
        public OsType OsType { get; set; }

        public virtual User User { get; set; }
    }
}