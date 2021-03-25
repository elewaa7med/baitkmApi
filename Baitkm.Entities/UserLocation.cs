using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class UserLocation : EntityBase
    {
        public int UserId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public string IpAddress { get; set; }

        public virtual User User { get; set; }
    }
}