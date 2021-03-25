using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class PhoneCode : EntityBase
    {
        public string Country { get; set; }
        public string Code { get; set; }
    }
}