using Baitkm.Entities.Base;

namespace Baitkm.Entities
{
    public class Rate : EntityBase
    {
        public decimal CurrentRate { get; set; }
        public int CurrencyId { get; set; }
    }
}